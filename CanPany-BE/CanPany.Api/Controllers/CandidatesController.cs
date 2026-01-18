using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
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
            var result = candidates.Select(c => new 
            {
                c.Profile,
                c.MatchScore
            });
            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage("CandidatesRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToSearchCandidates"), "SearchCandidatesFailed"));
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
            
            var result = candidates.Select(c => new 
            {
                c.Profile,
                c.MatchScore
            });

            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage("CandidatesRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching candidates with filters");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToSearchCandidates"), "SearchCandidatesFailed"));
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
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage("CandidateNotFound"), "NotFound"));

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

            return Ok(ApiResponse.CreateSuccess(candidateInfo, _i18nService.GetDisplayMessage("CandidateRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetCandidate"), "GetCandidateFailed"));
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
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage("CandidateNotFound"), "NotFound"));

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
                    cv.ExtractedSkills
                })
            };

            return Ok(ApiResponse.CreateSuccess(fullProfile, _i18nService.GetDisplayMessage("CandidateProfileRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate profile");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetCandidateProfile"), "GetCandidateProfileFailed"));
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
                return StatusCode(403, ApiResponse.CreateError(_i18nService.GetErrorMessage("UnlockRequired"), "UnlockRequired"));

            var cvs = await _cvService.GetByUserIdAsync(id);
            return Ok(ApiResponse.CreateSuccess(cvs, _i18nService.GetDisplayMessage("CandidateCVsRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate CVs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetCandidateCVs"), "GetCandidateCVsFailed"));
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

            return Ok(ApiResponse.CreateSuccess(applications, _i18nService.GetDisplayMessage("ApplicationsRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate applications");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetApplications"), "GetApplicationsFailed"));
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

            return Ok(ApiResponse.CreateSuccess(statistics, _i18nService.GetDisplayMessage("StatisticsRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting candidate statistics");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetStatistics"), "GetStatisticsFailed"));
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
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage("UnlockFailed"), "UnlockFailed"));

            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage("CandidateContactUnlockedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking candidate");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToUnlockCandidate"), "UnlockCandidateFailed"));
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
            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage("UnlockedCandidatesRetrievedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unlocked candidates");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage("FailedToGetUnlockedCandidates"), "GetUnlockedCandidatesFailed"));
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
