using System.Text.RegularExpressions;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.SemanticSearch;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Candidate search service implementation
/// </summary>
public class CandidateSearchService : ICandidateSearchService
{
    private readonly IJobRepository _jobRepo;
    private readonly IUserProfileRepository _profileRepo;
    private readonly IUserRepository _userRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly ISkillRepository _skillRepo;
    private readonly ITextEmbeddingService _embedding;
    private readonly ILogger<CandidateSearchService> _logger;
    
    // In-memory storage for unlocked candidates (should be moved to database in production)
    private static readonly Dictionary<string, HashSet<string>> _unlockedCandidates = new();

    public CandidateSearchService(
        IJobRepository jobRepo,
        IUserProfileRepository profileRepo,
        IUserRepository userRepo,
        IApplicationRepository applicationRepo,
        ISkillRepository skillRepo,
        ITextEmbeddingService embedding,
        ILogger<CandidateSearchService> logger)
    {
        _jobRepo = jobRepo;
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _applicationRepo = applicationRepo;
        _skillRepo = skillRepo;
        _embedding = embedding;
        _logger = logger;
    }

    public async Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesAsync(string jobId, int limit = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                return Enumerable.Empty<(UserProfile, double)>();

            // Get all user profiles
            var allProfiles = await _profileRepo.GetAllAsync();
            var jobSkillIds = new HashSet<string>(job.SkillIds ?? new List<string>());

            var candidatesWithScore = new List<(UserProfile Profile, double MatchScore)>();

            foreach (var profile in allProfiles)
            {
                var profileSkillIds = new HashSet<string>(profile.SkillIds ?? new List<string>());
                
                if (profileSkillIds.Count == 0)
                    continue;

                // Calculate match score based on skill overlap
                var matchedSkills = jobSkillIds.Intersect(profileSkillIds).Count();
                var totalJobSkills = jobSkillIds.Count;
                
                if (totalJobSkills == 0)
                    continue;

                var matchScore = (double)matchedSkills / totalJobSkills * 100.0;
                candidatesWithScore.Add((profile, matchScore));
            }

            return candidatesWithScore
                .OrderByDescending(x => x.MatchScore)
                .Take(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates: {JobId}", jobId);
            throw;
        }
    }

    public async Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesSemanticAsync(
        string jobId,
        int limit = 20,
        string? location = null,
        string? experienceLevel = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            if (!IsValidObjectId(jobId))
                throw new ArgumentException("Invalid Job ID format", nameof(jobId));

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                return Enumerable.Empty<(UserProfile, double)>();

            // Build job semantic text (description + title + skill names)
            var jobSkillIds = job.SkillIds ?? new List<string>();
            var skillNameMap = await LoadSkillNameMapAsync(jobSkillIds);
            var jobText = BuildJobText(job, jobSkillIds, skillNameMap);

            // Ensure job has an embedding cached
            var jobVec = job.SkillEmbedding;
            if (jobVec == null || jobVec.Count == 0)
            {
                jobVec = _embedding.Embed(jobText);
                job.SkillEmbedding = jobVec;
                try 
                {
                    await _jobRepo.UpdateAsync(job);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update job embedding cache for {JobId}", job.Id);
                    // Continue without saving - don't fail the request
                }
            }

            var allProfiles = await _profileRepo.GetAllAsync();
            var candidateUsers = await _userRepo.GetByRoleAsync("Candidate");
            var candidateUserSet = new HashSet<string>(candidateUsers.Select(u => u.Id));

            // Apply filters first (location/experience level)
            IEnumerable<UserProfile> filteredProfiles = allProfiles.Where(p => candidateUserSet.Contains(p.UserId));
            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredProfiles = filteredProfiles.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Location) && p.Location.Contains(location, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.Address) && p.Address.Contains(location, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                filteredProfiles = filteredProfiles.Where(p => ExperienceMatchesLevel(p, experienceLevel));
            }

            var candidatesWithScore = new List<(UserProfile Profile, double MatchScore)>();
            var jobSkillSet = new HashSet<string>(jobSkillIds);

            // Preload skill names used by candidate profiles for better embeddings
            var allSkillIdsNeeded = filteredProfiles.SelectMany(p => p.SkillIds ?? new List<string>()).Distinct().ToList();
            var allSkillNameMap = await LoadSkillNameMapAsync(allSkillIdsNeeded);

            foreach (var profile in filteredProfiles)
            {
                // Ensure profile has embedding cached
                var profileVec = profile.SkillEmbedding;
                if (profileVec == null || profileVec.Count == 0)
                {
                    var profileText = BuildProfileText(profile, allSkillNameMap);
                    profileVec = _embedding.Embed(profileText);
                    profile.SkillEmbedding = profileVec;
                    try
                    {
                        await _profileRepo.UpdateAsync(profile);
                    }
                    catch (Exception ex)
                    {
                         _logger.LogWarning(ex, "Failed to update profile embedding cache for {ProfileId}", profile.Id);
                    }
                }

                // 1) Semantic similarity (cosine) -> [0..1]
                var cosine = VectorMath.CosineSimilarity(jobVec, profileVec);
                cosine = Math.Clamp(cosine, -1.0, 1.0);
                var semanticScore = (cosine + 1.0) / 2.0; // map [-1..1] -> [0..1]

                // 2) Skill overlap score -> [0..1]
                var profileSkillSet = new HashSet<string>(profile.SkillIds ?? new List<string>());
                var overlap = jobSkillSet.Count == 0 ? 0.0 : (double)jobSkillSet.Intersect(profileSkillSet).Count() / jobSkillSet.Count;

                // 3) Experience relevance score -> [0..1]
                var expScore = ExperienceRelevance(job.Level, profile);

                // Weighted blend -> percent
                var blended = 0.65 * semanticScore + 0.25 * overlap + 0.10 * expScore;
                var matchScore = Math.Round(blended * 100.0, 2);

                candidatesWithScore.Add((profile, matchScore));
            }

            return candidatesWithScore
                .OrderByDescending(x => x.MatchScore)
                .Take(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates semantically: {JobId}", jobId);
            throw;
        }
    }

    private async Task<Dictionary<string, string>> LoadSkillNameMapAsync(IEnumerable<string> skillIds)
    {
        var ids = skillIds.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<string, string>();

        var all = await _skillRepo.GetAllAsync();
        // This repo doesn't have a bulk get-by-ids; map locally (OK for small datasets).
        return all.Where(s => ids.Contains(s.Id)).ToDictionary(s => s.Id, s => s.Name);
    }

    private static string BuildJobText(Job job, List<string> skillIds, IReadOnlyDictionary<string, string> skillNameMap)
    {
        var skillNames = skillIds.Select(id => skillNameMap.TryGetValue(id, out var n) ? n : id);
        return string.Join(" | ", new[]
        {
            job.Title ?? string.Empty,
            job.Description ?? string.Empty,
            $"Level: {job.Level ?? string.Empty}",
            $"Location: {job.Location ?? string.Empty}",
            "Skills: " + string.Join(", ", skillNames)
        });
    }

    private static string BuildProfileText(UserProfile profile, IReadOnlyDictionary<string, string> skillNameMap)
    {
        var skillNames = (profile.SkillIds ?? new List<string>())
            .Select(id => skillNameMap.TryGetValue(id, out var n) ? n : id);

        return string.Join(" | ", new[]
        {
            profile.Title ?? string.Empty,
            profile.Bio ?? string.Empty,
            profile.Experience ?? string.Empty,
            profile.Education ?? string.Empty,
            $"Location: {profile.Location ?? profile.Address ?? string.Empty}",
            "Skills: " + string.Join(", ", skillNames)
        });
    }

    private static bool ExperienceMatchesLevel(UserProfile profile, string experienceLevel)
    {
        var level = experienceLevel.Trim().ToLowerInvariant();
        var text = $"{profile.Title} {profile.Experience}".ToLowerInvariant();

        // Textual hint
        if (level is "junior" or "mid" or "senior" or "expert")
        {
            if (text.Contains(level)) return true;
        }

        // Parse years from experience text (first integer)
        var years = ParseYears(profile.Experience);
        return level switch
        {
            "junior" => years is null ? false : years <= 2,
            "mid" => years is null ? false : years is >= 3 and <= 5,
            "senior" => years is null ? false : years is >= 6 and <= 9,
            "expert" => years is null ? false : years >= 10,
            _ => true // unknown filter value -> don't filter out
        };
    }

    private static double ExperienceRelevance(string? jobLevel, UserProfile profile)
    {
        if (string.IsNullOrWhiteSpace(jobLevel)) return 0.5;
        var level = jobLevel.Trim().ToLowerInvariant();
        var years = ParseYears(profile.Experience);
        if (years is null)
        {
            // fallback to title keyword
            var text = $"{profile.Title} {profile.Experience}".ToLowerInvariant();
            return text.Contains(level) ? 1.0 : 0.5;
        }

        return level switch
        {
            "junior" => years <= 2 ? 1.0 : years <= 4 ? 0.6 : 0.3,
            "mid" => years is >= 3 and <= 6 ? 1.0 : years <= 2 ? 0.6 : years <= 9 ? 0.7 : 0.4,
            "senior" => years >= 6 ? 1.0 : years >= 4 ? 0.7 : 0.4,
            "expert" => years >= 10 ? 1.0 : years >= 7 ? 0.7 : 0.4,
            _ => 0.5
        };
    }

    private static int? ParseYears(string? experienceText)
    {
        if (string.IsNullOrWhiteSpace(experienceText)) return null;
        foreach (var part in experienceText.Split(new[] { ' ', ',', ';', '.', '/', '-', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, out var n) && n >= 0 && n <= 80)
                return n;
        }
        return null;
    }

    public async Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesWithFiltersAsync(
        string? keyword,
        List<string>? skillIds,
        string? location,
        decimal? minHourlyRate,
        decimal? maxHourlyRate,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var allProfiles = await _profileRepo.GetAllAsync();
            var allUsers = await _userRepo.GetByRoleAsync("Candidate");
            var userDict = allUsers.ToDictionary(u => u.Id);

            var filteredProfiles = allProfiles
                .Where(p => userDict.ContainsKey(p.UserId))
                .AsEnumerable();

            // Apply keyword filter
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filteredProfiles = filteredProfiles.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Bio) && p.Bio.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.Title) && p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.Experience) && p.Experience.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply skill filter
            if (skillIds != null && skillIds.Any())
            {
                var skillSet = new HashSet<string>(skillIds);
                filteredProfiles = filteredProfiles.Where(p =>
                    p.SkillIds != null && p.SkillIds.Any(s => skillSet.Contains(s)));
            }

            // Apply location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredProfiles = filteredProfiles.Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Location) && p.Location.Contains(location, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.Address) && p.Address.Contains(location, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply hourly rate filter
            if (minHourlyRate.HasValue)
            {
                filteredProfiles = filteredProfiles.Where(p => p.HourlyRate >= minHourlyRate.Value);
            }

            if (maxHourlyRate.HasValue)
            {
                filteredProfiles = filteredProfiles.Where(p => p.HourlyRate <= maxHourlyRate.Value);
            }

            // Calculate match scores (simplified - based on skill overlap if skillIds provided)
            var candidatesWithScore = new List<(UserProfile Profile, double MatchScore)>();
            var skillSetForScoring = skillIds != null ? new HashSet<string>(skillIds) : null;

            foreach (var profile in filteredProfiles)
            {
                double matchScore = 100.0; // Default score

                if (skillSetForScoring != null && profile.SkillIds != null && profile.SkillIds.Any())
                {
                    var profileSkillSet = new HashSet<string>(profile.SkillIds);
                    var matchedSkills = skillSetForScoring.Intersect(profileSkillSet).Count();
                    var totalSkills = skillSetForScoring.Count;
                    matchScore = totalSkills > 0 ? (double)matchedSkills / totalSkills * 100.0 : 0;
                }

                candidatesWithScore.Add((profile, matchScore));
            }

            // Pagination
            return candidatesWithScore
                .OrderByDescending(x => x.MatchScore)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates with filters");
            throw;
        }
    }

    public async Task<bool> UnlockCandidateContactAsync(string companyId, string candidateId)
    {
        try
        {
            // TODO: Implement unlock logic with premium package check and credit deduction
            // This should check if company has premium package and deduct credits
            
            // Store unlock relationship (in-memory for now, should be in database)
            if (!_unlockedCandidates.ContainsKey(companyId))
            {
                _unlockedCandidates[companyId] = new HashSet<string>();
            }
            
            _unlockedCandidates[companyId].Add(candidateId);
            
            _logger.LogInformation("Unlocking candidate contact: {CompanyId}, {CandidateId}", companyId, candidateId);
            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking candidate contact: {CompanyId}, {CandidateId}", companyId, candidateId);
            return false;
        }
    }

    public async Task<bool> HasUnlockedCandidateAsync(string companyId, string candidateId)
    {
        try
        {
            // Check if company has unlocked this candidate
            await Task.CompletedTask;
            if (_unlockedCandidates.ContainsKey(companyId))
            {
                return _unlockedCandidates[companyId].Contains(candidateId);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking unlock status: {CompanyId}, {CandidateId}", companyId, candidateId);
            return false;
        }
    }

    public async Task<IEnumerable<(User User, UserProfile Profile)>> GetUnlockedCandidatesAsync(string companyId, int page = 1, int pageSize = 20)
    {
        try
        {
            if (!_unlockedCandidates.ContainsKey(companyId))
            {
                return Enumerable.Empty<(User, UserProfile)>();
            }

            var unlockedCandidateIds = _unlockedCandidates[companyId]
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<(User User, UserProfile Profile)>();

            foreach (var candidateId in unlockedCandidateIds)
            {
                var user = await _userRepo.GetByIdAsync(candidateId);
                if (user != null && user.Role == "Candidate")
                {
                    var profile = await _profileRepo.GetByUserIdAsync(candidateId);
                    result.Add((user, profile!));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unlocked candidates: {CompanyId}", companyId);
            throw;
        }
    }
    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && Regex.IsMatch(id, "^[0-9a-fA-F]{24}$");
    }
}


