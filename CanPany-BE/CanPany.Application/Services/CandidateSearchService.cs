using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.DTOs;
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
    private readonly IGeminiService _geminiService;
    private readonly IUnlockRecordRepository _unlockRecordRepo;
    private readonly IWalletService _walletService;
    private readonly ILogger<CandidateSearchService> _logger;
    
    // Default unlock fee (could be moved to settings/config)
    private const long UnlockFee = 50000; // e.g. 50,000 VND

    public CandidateSearchService(
        IJobRepository jobRepo,
        IUserProfileRepository profileRepo,
        IUserRepository userRepo,
        IApplicationRepository applicationRepo,
        ISkillRepository skillRepo,
        IGeminiService geminiService,
        IUnlockRecordRepository unlockRecordRepo,
        IWalletService walletService,
        ILogger<CandidateSearchService> logger)
    {
        _jobRepo = jobRepo;
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _applicationRepo = applicationRepo;
        _skillRepo = skillRepo;
        _geminiService = geminiService;
        _unlockRecordRepo = unlockRecordRepo;
        _walletService = walletService;
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

            var jobSkills = job.SkillIds ?? new List<string>();

            // If job has skills, find candidates by skill overlap
            IEnumerable<UserProfile> candidates;
            if (jobSkills.Any())
            {
                candidates = await _profileRepo.GetBySkillIdsAsync(jobSkills, limit * 3);
            }
            else
            {
                // No skills on job, get recent profiles
                candidates = await _profileRepo.GetAllAsync();
            }

            // Filter to Candidate role only
            var allCandidateUsers = await _userRepo.GetByRoleAsync("Candidate");
            var candidateUserIds = new HashSet<string>(allCandidateUsers.Select(u => u.Id));

            // Score by skill overlap + text relevance
            var jobSkillSet = new HashSet<string>(jobSkills);
            var jobKeywords = $"{job.Title} {job.Description}".ToLowerInvariant();

            var scored = candidates
                .Where(p => candidateUserIds.Contains(p.UserId))
                .Select(p =>
                {
                    double score = 0;

                    // Skill overlap score (0-70 points)
                    if (jobSkillSet.Count > 0 && p.SkillIds != null)
                    {
                        var matched = p.SkillIds.Count(s => jobSkillSet.Contains(s));
                        score += (double)matched / jobSkillSet.Count * 70;
                    }

                    // Text relevance bonus (0-30 points)
                    var profileText = $"{p.Title} {p.Bio} {p.Experience}".ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(job.Title) && profileText.Contains(job.Title.ToLowerInvariant()))
                        score += 15;
                    if (!string.IsNullOrWhiteSpace(p.Location) && !string.IsNullOrWhiteSpace(job.Location) &&
                        p.Location.Contains(job.Location, StringComparison.OrdinalIgnoreCase))
                        score += 15;

                    return (Profile: p, MatchScore: Math.Min(score, 100));
                })
                .Where(x => x.MatchScore > 0)
                .OrderByDescending(x => x.MatchScore)
                .Take(limit)
                .ToList();

            _logger.LogInformation("[DB_SEARCH] SearchCandidatesAsync for job {JobId}: found {Count} candidates", jobId, scored.Count);

            return scored;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates: {JobId}", jobId);
            throw;
        }
    }

    public async Task<IEnumerable<SemanticSearchResponse>> SemanticSearchAsync(SemanticSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.JobDescription))
                throw new ArgumentException("Job description cannot be empty", nameof(request.JobDescription));

            // 1. Generate embedding for job description
            var embedding = await _geminiService.GenerateEmbeddingAsync(request.JobDescription);

            // 2. Search by vector
            var vectorResults = await _profileRepo.SearchByVectorAsync(embedding, limit: request.Limit * 2);

            // 3. Apply filters and calculate scores
            var finalResults = new List<SemanticSearchResponse>();
            
            foreach (var (profile, score) in vectorResults)
            {
                // Basic location filtering (in-memory)
                if (!string.IsNullOrEmpty(request.Location) && 
                    !string.IsNullOrEmpty(profile.Location) && 
                    !profile.Location.Contains(request.Location, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Basic experience filtering (in-memory)
                if (!string.IsNullOrEmpty(request.ExperienceLevel) && 
                    !string.IsNullOrEmpty(profile.Experience) && 
                    !profile.Experience.Contains(request.ExperienceLevel, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Get User Info
                var user = await _userRepo.GetByIdAsync(profile.UserId);
                if (user == null) continue;

                // 4. Calculate multi-factor match score
                // Base score from semantic vector similarity (70%)
                double matchScore = score * 100;

                // Skill overlap bonus (20%)
                if (profile.SkillIds != null && profile.SkillIds.Any())
                {
                    int overlapCount = 0;
                    foreach (var skillId in profile.SkillIds)
                    {
                        var skill = await _skillRepo.GetByIdAsync(skillId);
                        if (skill != null && request.JobDescription.Contains(skill.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            overlapCount++;
                        }
                    }
                    
                    double skillBonus = Math.Min(overlapCount * 5, 20); // 5 points per skill, max 20
                    matchScore += skillBonus;
                }

                // Experience relevance bonus (10%)
                if (!string.IsNullOrEmpty(request.ExperienceLevel) && 
                    !string.IsNullOrEmpty(profile.Experience) && 
                    profile.Experience.Contains(request.ExperienceLevel, StringComparison.OrdinalIgnoreCase))
                {
                    matchScore += 10;
                }

                finalResults.Add(new SemanticSearchResponse(
                    Profile: new UserProfileDto 
                    { 
                        Id = profile.Id,
                        UserId = profile.UserId,
                        Title = profile.Title ?? string.Empty,
                        Bio = profile.Bio ?? string.Empty,
                        Location = profile.Location,
                        HourlyRate = profile.HourlyRate,
                        Languages = profile.Languages,
                        Certifications = profile.Certifications,
                        SkillIds = profile.SkillIds
                    },
                    MatchScore: Math.Min(matchScore, 100),
                    UserInfo: new UserBasicInfo(user.Id, user.FullName, user.Email!, user.AvatarUrl)
                ));
            }

            var sortedResults = finalResults
                .OrderByDescending(x => x.MatchScore)
                .Take(request.Limit)
                .ToList();

            // 5. RAG step — send top N candidates to Gemini for AI reasoning
            const int ragLimit = 10;
            var candidatesForRag = sortedResults.Take(ragLimit).ToList();

            if (candidatesForRag.Count > 0)
            {
                try
                {
                    var summaries = candidatesForRag.Select(c => new CandidateSummaryForRanking(
                        UserId: c.UserInfo.Id,
                        FullName: c.UserInfo.FullName,
                        Title: c.Profile.Title,
                        Bio: c.Profile.Bio,
                        Experience: null, // Not in DTO — vector score carries this weight
                        Location: c.Profile.Location,
                        Skills: c.Profile.SkillIds ?? new List<string>(),
                        VectorScore: c.MatchScore
                    )).ToList();

                    var ragResults = await _geminiService.RankCandidatesAsync(
                        request.JobDescription, summaries);

                    if (ragResults.Count > 0)
                    {
                        var ragLookup = ragResults.ToDictionary(r => r.UserId, r => r);

                        // Merge AI reasoning back into results
                        sortedResults = sortedResults.Select(r =>
                        {
                            if (ragLookup.TryGetValue(r.UserInfo.Id, out var rag))
                            {
                                return r with
                                {
                                    AiReason = rag.Reason,
                                    MatchScore = rag.AdjustedScore
                                };
                            }
                            return r;
                        })
                        .OrderByDescending(r => r.MatchScore)
                        .ToList();

                        _logger.LogInformation("[SEMANTIC_RAG] Merged AI reasoning for {Count} candidates", ragResults.Count);
                    }
                }
                catch (Exception ex)
                {
                    // Graceful degradation: RAG failed, return vector-only results
                    _logger.LogWarning(ex, "[SEMANTIC_RAG_FALLBACK] RAG ranking failed, returning vector-only results");
                }
            }

            return sortedResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing semantic search");
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
            // Resolve keyword → matching skill IDs (so "C#" finds candidates with C# skill)
            List<string>? mergedSkillIds = skillIds != null ? new List<string>(skillIds) : null;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var allSkills = await _skillRepo.GetAllAsync();
                var matchedSkillIds = allSkills
                    .Where(s => s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Id)
                    .ToList();

                if (matchedSkillIds.Any())
                {
                    mergedSkillIds ??= new List<string>();
                    mergedSkillIds.AddRange(matchedSkillIds);
                    mergedSkillIds = mergedSkillIds.Distinct().ToList();
                    _logger.LogInformation("[DB_SEARCH] Keyword '{Keyword}' matched {Count} skills: {SkillIds}", 
                        keyword, matchedSkillIds.Count, string.Join(", ", matchedSkillIds));
                }
            }

            // Pure DB search — no embeddings needed
            // Pass both keyword (text search) and merged skill IDs (includes keyword-matched skills)
            var profiles = await _profileRepo.SearchByFiltersAsync(
                keyword, mergedSkillIds, location, experience, minHourlyRate, maxHourlyRate, page, pageSize);

            // Filter to Candidate role only
            var allCandidateUsers = await _userRepo.GetByRoleAsync("Candidate");
            var candidateUserIds = new HashSet<string>(allCandidateUsers.Select(u => u.Id));

            var candidateProfiles = profiles.Where(p => candidateUserIds.Contains(p.UserId)).ToList();

            // Calculate match scores based on how many filter criteria match
            var scoredResults = candidateProfiles.Select(profile =>
            {
                double score = 0;
                int criteriaCount = 0;

                // Keyword match score
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    criteriaCount++;
                    var kw = keyword.ToLowerInvariant();
                    var textFields = $"{profile.Title} {profile.Bio} {profile.Experience} {profile.Education} {profile.Location}".ToLowerInvariant();
                    if (textFields.Contains(kw)) score += 25;
                }

                // Skill match score
                if (skillIds != null && skillIds.Any())
                {
                    criteriaCount++;
                    if (profile.SkillIds != null)
                    {
                        var skillSet = new HashSet<string>(skillIds);
                        var matchedCount = profile.SkillIds.Count(s => skillSet.Contains(s));
                        score += (double)matchedCount / skillIds.Count * 40; // Up to 40 points
                    }
                }

                // Location match score
                if (!string.IsNullOrWhiteSpace(location))
                {
                    criteriaCount++;
                    if ((!string.IsNullOrWhiteSpace(profile.Location) && profile.Location.Contains(location, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(profile.Address) && profile.Address.Contains(location, StringComparison.OrdinalIgnoreCase)))
                        score += 20;
                }

                // Experience match score
                if (!string.IsNullOrWhiteSpace(experience))
                {
                    criteriaCount++;
                    if (!string.IsNullOrWhiteSpace(profile.Experience) && profile.Experience.Contains(experience, StringComparison.OrdinalIgnoreCase))
                        score += 15;
                }

                // If no filters were specified, give a base score based on profile completeness
                if (criteriaCount == 0)
                {
                    score = 50; // Base score for all profiles when no filters
                    if (!string.IsNullOrWhiteSpace(profile.Bio)) score += 10;
                    if (profile.SkillIds != null && profile.SkillIds.Any()) score += 15;
                    if (!string.IsNullOrWhiteSpace(profile.Experience)) score += 15;
                    if (!string.IsNullOrWhiteSpace(profile.Title)) score += 10;
                }

                return (Profile: profile, MatchScore: Math.Min(score, 100));
            })
            .OrderByDescending(x => x.MatchScore)
            .ToList();

            _logger.LogInformation("[DB_SEARCH] SearchCandidatesWithFiltersAsync: {Count} results (keyword={Keyword}, skillIds={SkillCount})", 
                scoredResults.Count, keyword ?? "none", skillIds?.Count ?? 0);

            return scoredResults;
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
            // Check if already unlocked
            var alreadyUnlocked = await _unlockRecordRepo.HasUnlockedAsync(companyId, candidateId);
            if (alreadyUnlocked) return true;

            // Check wallet balance and deduct
            var (success, errors, wallet) = await _walletService.ChangeBalanceAsync(companyId, -UnlockFee, $"Unlock candidate contact: {candidateId}");

            if (!success)
            {
                _logger.LogWarning("Failed to unlock candidate {CandidateId} for company {CompanyId}. Reason: {Reason}", candidateId, companyId, string.Join(", ", errors));
                return false;
            }

            // Create unlock record
            var unlockRecord = new UnlockRecord
            {
                CompanyId = companyId,
                CandidateId = candidateId,
                FeeAmount = UnlockFee,
                UnlockedAt = DateTime.UtcNow
            };
            
            await _unlockRecordRepo.AddAsync(unlockRecord);
            
            _logger.LogInformation("Company {CompanyId} unlocked candidate {CandidateId} for {Fee}", companyId, candidateId, UnlockFee);
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
            return await _unlockRecordRepo.HasUnlockedAsync(companyId, candidateId);
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
            var unlockRecords = await _unlockRecordRepo.GetByCompanyIdAsync(companyId, page, pageSize);
            var result = new List<(User User, UserProfile Profile)>();

            foreach (var record in unlockRecords)
            {
                var user = await _userRepo.GetByIdAsync(record.CandidateId);
                if (user != null && user.Role == "Candidate")
                {
                    var profile = await _profileRepo.GetByUserIdAsync(record.CandidateId);
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


