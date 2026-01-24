using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
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
    private readonly IGeminiService _geminiService;
    private readonly ILogger<CandidateSearchService> _logger;
    
    // In-memory storage for unlocked candidates (should be moved to database in production)
    private static readonly Dictionary<string, HashSet<string>> _unlockedCandidates = new();

    public CandidateSearchService(
        IJobRepository jobRepo,
        IUserProfileRepository profileRepo,
        IUserRepository userRepo,
        IApplicationRepository applicationRepo,
        IGeminiService geminiService,
        ILogger<CandidateSearchService> logger)
    {
        _jobRepo = jobRepo;
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _applicationRepo = applicationRepo;
        _geminiService = geminiService;
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

            // Generate embedding for job description and skills
            var jobText = $"{job.Title} {job.Description} {string.Join(" ", job.SkillIds ?? new List<string>())}";
            var embedding = await _geminiService.GenerateEmbeddingAsync(jobText);

            // Search by vector
            var results = await _profileRepo.SearchByVectorAsync(embedding, limit);

            return results.Select(r => (r.Profile, MatchScore: r.Score * 100)); // Convert 0-1 score to 0-100
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates: {JobId}", jobId);
            throw;
        }
    }

    public async Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesWithFiltersAsync(
        string? keyword,
        List<string>? skillIds,
        string? location,
        string? experience,
        decimal? minHourlyRate,
        decimal? maxHourlyRate,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var candidatesWithScore = new List<(UserProfile Profile, double MatchScore)>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Vector search for keyword
                var embedding = await _geminiService.GenerateEmbeddingAsync(keyword);
                var vectorResults = await _profileRepo.SearchByVectorAsync(embedding, limit: 100); // Get top 100 semantic matches
                
                foreach (var result in vectorResults)
                {
                    candidatesWithScore.Add((result.Profile, result.Score * 100));
                }
            }
            else
            {
                // No keyword, get all profiles
                var allProfiles = await _profileRepo.GetAllAsync();
                foreach (var profile in allProfiles)
                {
                    candidatesWithScore.Add((profile, 0)); // Default score, will be updated by skill match
                }
            }

            // Apply filters (in-memory)
            var filteredCandidates = candidatesWithScore.AsEnumerable();

            // Filter specific user role
            var allUsers = await _userRepo.GetByRoleAsync("Candidate");
            var userDict = allUsers.ToDictionary(u => u.Id);
            filteredCandidates = filteredCandidates.Where(c => userDict.ContainsKey(c.Profile.UserId));

            // Apply skill filter
            if (skillIds != null && skillIds.Any())
            {
                var skillSet = new HashSet<string>(skillIds);
                filteredCandidates = filteredCandidates.Where(c =>
                    c.Profile.SkillIds != null && c.Profile.SkillIds.Any(s => skillSet.Contains(s)));
            }

            // Apply location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredCandidates = filteredCandidates.Where(c =>
                    (!string.IsNullOrWhiteSpace(c.Profile.Location) && c.Profile.Location.Contains(location, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(c.Profile.Address) && c.Profile.Address.Contains(location, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply experience filter
            if (!string.IsNullOrWhiteSpace(experience))
            {
                filteredCandidates = filteredCandidates.Where(c =>
                    !string.IsNullOrWhiteSpace(c.Profile.Experience) && c.Profile.Experience.Contains(experience, StringComparison.OrdinalIgnoreCase));
            }

            // Apply hourly rate filter
            if (minHourlyRate.HasValue)
            {
                filteredCandidates = filteredCandidates.Where(c => c.Profile.HourlyRate >= minHourlyRate.Value);
            }

            if (maxHourlyRate.HasValue)
            {
                filteredCandidates = filteredCandidates.Where(c => c.Profile.HourlyRate <= maxHourlyRate.Value);
            }

            // Recalculate match scores based on skills if filters heavily rely on them?
            // Or keep vector score if available?
            // Simple logic: If Vector Score exists (>0), use it. Else calculate skill overlap.
            
            var finalResults = new List<(UserProfile Profile, double MatchScore)>();
            
            foreach (var (profile, vectorScore) in filteredCandidates)
            {
                double finalScore = vectorScore;

                if (skillIds != null && skillIds.Any())
                {
                    // Boost score if skills match exact IDs requested
                    var skillSetForScoring = new HashSet<string>(skillIds);
                     if (profile.SkillIds != null)
                     {
                        var matchedSkills = skillSetForScoring.Intersect(profile.SkillIds).Count();
                        var skillScore = (double)matchedSkills / skillSetForScoring.Count * 100.0;
                        if (finalScore == 0) finalScore = skillScore; // If no keyword search, use skill score
                        else finalScore = (finalScore * 0.7) + (skillScore * 0.3); // Weighted average
                     }
                }
                
                finalResults.Add((profile, finalScore));
            }

            // Pagination
            return finalResults
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
}


