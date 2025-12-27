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
    private readonly ILogger<CandidateSearchService> _logger;
    
    // In-memory storage for unlocked candidates (should be moved to database in production)
    private static readonly Dictionary<string, HashSet<string>> _unlockedCandidates = new();

    public CandidateSearchService(
        IJobRepository jobRepo,
        IUserProfileRepository profileRepo,
        IUserRepository userRepo,
        IApplicationRepository applicationRepo,
        ILogger<CandidateSearchService> logger)
    {
        _jobRepo = jobRepo;
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _applicationRepo = applicationRepo;
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


