using CanPany.Application.Interfaces.Services;
using CanPany.Application.DTOs;
using CanPany.Application.Common.Models;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Candidates controller - UC-CMP-12, UC-CMP-13 and additional candidate management APIs
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateSearchService _candidateSearchService;
    private readonly IUserService _userService;
    private readonly IUserProfileService _userProfileService;
    private readonly ICVService _cvService;
    private readonly IApplicationService _applicationService;
    private readonly ILogger<CandidatesController> _logger;
    private readonly II18nService _i18nService;

    public CandidatesController(
        ICandidateSearchService candidateSearchService,
        IUserService userService,
        IUserProfileService userProfileService,
        ICVService cvService,
        IApplicationService applicationService,
        ILogger<CandidatesController> logger,
        II18nService i18nService)
    {
        _candidateSearchService = candidateSearchService;
        _userService = userService;
        _userProfileService = userProfileService;
        _cvService = cvService;
        _applicationService = applicationService;
        _logger = logger;
        _i18nService = i18nService;
    }

    /// <summary>
    /// UC-CMP-12: Search Candidates by Job
    /// </summary>
    [HttpGet("search")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> SearchCandidates([FromQuery] string jobId, [FromQuery] int limit = 20)
    {
        try
        {
            var candidates = await _candidateSearchService.SearchCandidatesAsync(jobId, limit);
            var userIds = candidates.Select(c => c.Profile.UserId).Distinct().ToList();
            var users = new Dictionary<string, Domain.Entities.User>();
            foreach (var uid in userIds)
            {
                var u = await _userService.GetByIdAsync(uid);
                if (u != null) users[uid] = u;
            }
            var result = candidates.Select(c =>
            {
                users.TryGetValue(c.Profile.UserId, out var user);
                return new
                {
                    c.Profile,
                    c.MatchScore,
                    UserInfo = user != null ? new { user.Id, user.FullName, user.Email, user.AvatarUrl, user.Role } : null
                };
            });
            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (GeminiRateLimitException ex)
        {
            _logger.LogWarning(ex, "Gemini rate limit hit during candidate search");
            Response.Headers["Retry-After"] = ex.RetryAfterSeconds.ToString();
            return StatusCode(429, ApiResponse.CreateError(
                $"AI service is temporarily busy. Please try again in {ex.RetryAfterSeconds} seconds.",
                "RateLimited"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "SearchCandidatesFailed"));
        }
    }

    /// <summary>
    /// Search candidates with filters (for companies)
    /// </summary>
    [HttpGet("search/filters")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> SearchCandidatesWithFilters(
        [FromQuery] string? keyword,
        [FromQuery] List<string>? skillIds,
        [FromQuery] string? location,
        [FromQuery] string? experience,
        [FromQuery] decimal? minHourlyRate,
        [FromQuery] decimal? maxHourlyRate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var candidates = await _candidateSearchService.SearchCandidatesWithFiltersAsync(
                keyword, skillIds, location, experience, minHourlyRate, maxHourlyRate, page, pageSize);
            
            var userIds = candidates.Select(c => c.Profile.UserId).Distinct().ToList();
            var users = new Dictionary<string, Domain.Entities.User>();
            foreach (var uid in userIds)
            {
                var u = await _userService.GetByIdAsync(uid);
                if (u != null) users[uid] = u;
            }
            var result = candidates.Select(c =>
            {
                users.TryGetValue(c.Profile.UserId, out var user);
                return new
                {
                    c.Profile,
                    c.MatchScore,
                    UserInfo = user != null ? new { user.Id, user.FullName, user.Email, user.AvatarUrl, user.Role } : null
                };
            });

            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (GeminiRateLimitException ex)
        {
            _logger.LogWarning(ex, "Gemini rate limit hit during filter search");
            Response.Headers["Retry-After"] = ex.RetryAfterSeconds.ToString();
            return StatusCode(429, ApiResponse.CreateError(
                $"AI service is temporarily busy. Please try again in {ex.RetryAfterSeconds} seconds.",
                "RateLimited"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates with filters");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "SearchCandidatesFailed"));
        }
    }

    /// <summary>
    /// Semantic search for candidates based on job description (with RAG AI reasoning)
    /// </summary>
    [HttpPost("semantic-search")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> SemanticSearch([FromBody] SemanticSearchRequest request)
    {
        try
        {
            var results = await _candidateSearchService.SemanticSearchAsync(request);
            return Ok(ApiResponse.CreateSuccess(results, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (GeminiRateLimitException ex)
        {
            _logger.LogWarning(ex, "Gemini rate limit hit during semantic search");
            Response.Headers["Retry-After"] = ex.RetryAfterSeconds.ToString();
            return StatusCode(429, ApiResponse.CreateError(
                $"AI service is temporarily busy. Please try again in {ex.RetryAfterSeconds} seconds.",
                "RateLimited"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing semantic search");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "SemanticSearchFailed"));
        }
    }

    /// <summary>
    /// Get candidate by ID (public profile)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCandidate(string id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null || user.Role != "Candidate")
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.NotFound), "NotFound"));

            var profile = await _userProfileService.GetByUserIdAsync(id);
            
            var candidateInfo = new
            {
                Id = user.Id,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Profile = profile != null ? new
                {
                    Title = profile.Title,
                    Bio = profile.Bio,
                    Location = profile.Location,
                    HourlyRate = profile.HourlyRate,
                    Skills = profile.SkillIds
                } : null
            };

            return Ok(ApiResponse.CreateSuccess(candidateInfo, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.ProfileRetrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCandidateFailed"));
        }
    }

    /// <summary>
    /// Get candidate full profile (requires authentication - own profile or company/admin)
    /// </summary>
    [HttpGet("{id}/profile")]
    [Authorize]
    public async Task<IActionResult> GetCandidateProfile(string id)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            // Only allow if viewing own profile, or if company/admin
            if (currentUserId != id && currentUserRole != "Company" && currentUserRole != "Admin")
                return Forbid();

            var user = await _userService.GetByIdAsync(id);
            if (user == null || user.Role != "Candidate")
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.NotFound), "NotFound"));

            var profile = await _userProfileService.GetByUserIdAsync(id);
            var cvs = await _cvService.GetByUserIdAsync(id);

            var fullProfile = new
            {
                User = new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role
                },
                Profile = profile,
                CVs = cvs.Select(cv => new
                {
                    cv.Id,
                    cv.FileName,
                    cv.IsDefault,
                    cv.LatestAnalysisId,
                    cv.ExtractedSkills
                })
            };

            return Ok(ApiResponse.CreateSuccess(fullProfile, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.ProfileRetrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate profile");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCandidateProfileFailed"));
        }
    }

    /// <summary>
    /// Get candidate CVs (for companies with unlock permission)
    /// </summary>
    [HttpGet("{id}/cvs")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> GetCandidateCVs(string id)
    {
        try
        {
            var companyId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
                return Unauthorized();

            // Check if company has unlocked this candidate
            var hasUnlocked = await _candidateSearchService.HasUnlockedCandidateAsync(companyId, id);
            if (!hasUnlocked)
                return StatusCode(403, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "UnlockRequired"));

            var cvs = await _cvService.GetByUserIdAsync(id);
            return Ok(ApiResponse.CreateSuccess(cvs, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.CVsRetrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate CVs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCandidateCVsFailed"));
        }
    }

    /// <summary>
    /// Get candidate applications (own profile only)
    /// </summary>
    [HttpGet("{id}/applications")]
    [Authorize]
    public async Task<IActionResult> GetCandidateApplications(string id, [FromQuery] string? status = null)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            // Only allow viewing own applications
            if (currentUserId != id)
                return Forbid();

            var applications = await _applicationService.GetByCandidateIdAsync(id);
            
            if (!string.IsNullOrEmpty(status))
            {
                applications = applications.Where(a => a.Status == status);
            }

            return Ok(ApiResponse.CreateSuccess(applications, _i18nService.GetDisplayMessage(I18nKeys.Success.Application.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate applications");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetApplicationsFailed"));
        }
    }

    /// <summary>
    /// Get candidate statistics (own profile or company/admin)
    /// </summary>
    [HttpGet("{id}/statistics")]
    [Authorize]
    public async Task<IActionResult> GetCandidateStatistics(string id)
    {
        try
        {
            var currentUserId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst("role")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            // Only allow if viewing own stats, or if company/admin
            if (currentUserId != id && currentUserRole != "Company" && currentUserRole != "Admin")
                return Forbid();

            var applications = await _applicationService.GetByCandidateIdAsync(id);
            var cvs = await _cvService.GetByUserIdAsync(id);
            var profile = await _userProfileService.GetByUserIdAsync(id);

            var statistics = new
            {
                TotalApplications = applications.Count(),
                PendingApplications = applications.Count(a => a.Status == "Pending"),
                AcceptedApplications = applications.Count(a => a.Status == "Accepted"),
                RejectedApplications = applications.Count(a => a.Status == "Rejected"),
                TotalCVs = cvs.Count(),
                DefaultCV = cvs.FirstOrDefault(cv => cv.IsDefault),
                ProfileCompleteness = CalculateProfileCompleteness(profile),
                SkillsCount = profile?.SkillIds?.Count ?? 0
            };

            return Ok(ApiResponse.CreateSuccess(statistics, _i18nService.GetDisplayMessage(I18nKeys.Success.Statistics.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate statistics");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetStatisticsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-13: Unlock Candidate Contact Info
    /// </summary>
    [HttpPost("{candidateId}/unlock")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> UnlockCandidate(string candidateId)
    {
        try
        {
            var companyId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
                return Unauthorized();

            var succeeded = await _candidateSearchService.UnlockCandidateContactAsync(companyId, candidateId);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "UnlockFailed"));

            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.ContactUnlocked)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking candidate");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UnlockCandidateFailed"));
        }
    }

    /// <summary>
    /// Get list of unlocked candidates (for companies)
    /// </summary>
    [HttpGet("unlocked")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> GetUnlockedCandidates([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var companyId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(companyId))
                return Unauthorized();

            var unlockedCandidates = await _candidateSearchService.GetUnlockedCandidatesAsync(companyId, page, pageSize);
            var result = unlockedCandidates.Select(uc => new
            {
                uc.User,
                uc.Profile
            });
            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.UnlockedListRetrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unlocked candidates");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetUnlockedCandidatesFailed"));
        }
    }

    /// <summary>
    /// Admin: Check embedding health stats
    /// </summary>
    [HttpGet("admin/embedding-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEmbeddingStatus(
        [FromServices] IUserProfileRepository profileRepo)
    {
        try
        {
            var allProfiles = await profileRepo.GetAllAsync();
            var profileList = allProfiles.ToList();
            var withEmbedding = profileList.Count(p => p.Embedding != null && p.Embedding.Count > 0);
            var dims = profileList
                .Where(p => p.Embedding != null && p.Embedding.Count > 0)
                .GroupBy(p => p.Embedding!.Count)
                .Select(g => new { Dimension = g.Key, Count = g.Count() })
                .ToList();

            return Ok(ApiResponse.CreateSuccess(new
            {
                totalProfiles = profileList.Count,
                withEmbedding,
                withoutEmbedding = profileList.Count - withEmbedding,
                dimensionBreakdown = dims
            }, "Embedding status retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking embedding status");
            return StatusCode(500, ApiResponse.CreateError("Failed to get embedding status", "EmbeddingStatusFailed"));
        }
    }

    /// <summary>
    /// Admin: Re-generate all profile embeddings with current model
    /// </summary>
    [HttpPost("admin/regenerate-embeddings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegenerateEmbeddings(
        [FromServices] IGeminiService geminiService,
        [FromServices] IUserProfileRepository profileRepo)
    {
        try
        {
            var allProfiles = await profileRepo.GetAllAsync();
            var profileList = allProfiles.ToList();
            int updated = 0;
            int failed = 0;

            foreach (var profile in profileList)
            {
                try
                {
                    var text = $"{profile.Title} {profile.Bio} {profile.Experience} {profile.Location} {string.Join(" ", profile.SkillIds ?? new List<string>())}";
                    if (string.IsNullOrWhiteSpace(text.Trim())) continue;

                    var user = await _userService.GetByIdAsync(profile.UserId);
                    var fullName = user?.FullName ?? "";
                    text = $"{fullName} {text}";

                    var embedding = await geminiService.GenerateEmbeddingAsync(text);
                    profile.Embedding = embedding;
                    await profileRepo.UpdateAsync(profile);
                    updated++;

                    _logger.LogInformation("[REGEN_EMBED] Updated embedding for profile {Id} (dim={Dim})", profile.Id, embedding.Count);
                    
                    // Small delay to avoid rate limiting
                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    failed++;
                    _logger.LogWarning(ex, "[REGEN_EMBED] Failed for profile {Id}", profile.Id);
                    await Task.Delay(1000);
                }
            }

            return Ok(ApiResponse.CreateSuccess(new
            {
                total = profileList.Count,
                updated,
                failed
            }, $"Re-generated {updated} embeddings ({failed} failed)"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating embeddings");
            return StatusCode(500, ApiResponse.CreateError("Failed to regenerate embeddings", "RegenerateEmbeddingsFailed"));
        }
    }

    private int CalculateProfileCompleteness(UserProfile? profile)
    {
        if (profile == null) return 0;

        int totalFields = 10;
        int filledFields = 0;

        if (!string.IsNullOrWhiteSpace(profile.Bio)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.Phone)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.Address)) filledFields++;
        if (profile.DateOfBirth.HasValue) filledFields++;
        if (profile.SkillIds != null && profile.SkillIds.Any()) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.Experience)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.Education)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.Portfolio)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl)) filledFields++;
        if (!string.IsNullOrWhiteSpace(profile.GitHubUrl)) filledFields++;

        return (int)((double)filledFields / totalFields * 100);
    }
}
