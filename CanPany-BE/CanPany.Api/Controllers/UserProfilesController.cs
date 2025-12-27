using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// User Profiles controller - UC-CAN-01, UC-CAN-02, UC-CAN-03, UC-CAN-04
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileService _profileService;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(
        IUserProfileService profileService,
        ILogger<UserProfilesController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-01: View Personal Profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _profileService.GetByUserIdAsync(userId);
            if (profile == null)
                return NotFound(ApiResponse.CreateError("Profile not found", "NotFound"));

            return Ok(ApiResponse<UserProfile>.CreateSuccess(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile");
            return StatusCode(500, ApiResponse.CreateError("Failed to get profile", "GetProfileFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-02: Update Personal Profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _profileService.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Update profile fields
            if (!string.IsNullOrWhiteSpace(request.Bio)) profile.Bio = request.Bio;
            if (!string.IsNullOrWhiteSpace(request.Phone)) profile.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Address)) profile.Address = request.Address;
            if (request.DateOfBirth.HasValue) profile.DateOfBirth = request.DateOfBirth;
            if (request.SkillIds != null) profile.SkillIds = request.SkillIds;
            if (!string.IsNullOrWhiteSpace(request.Experience)) profile.Experience = request.Experience;
            if (!string.IsNullOrWhiteSpace(request.Education)) profile.Education = request.Education;
            if (!string.IsNullOrWhiteSpace(request.Portfolio)) profile.Portfolio = request.Portfolio;
            if (!string.IsNullOrWhiteSpace(request.LinkedInUrl)) profile.LinkedInUrl = request.LinkedInUrl;
            if (!string.IsNullOrWhiteSpace(request.GitHubUrl)) profile.GitHubUrl = request.GitHubUrl;
            if (!string.IsNullOrWhiteSpace(request.Title)) profile.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Location)) profile.Location = request.Location;
            if (request.HourlyRate.HasValue) profile.HourlyRate = request.HourlyRate;
            if (request.Languages != null) profile.Languages = request.Languages;
            if (request.Certifications != null) profile.Certifications = request.Certifications;

            await _profileService.UpdateAsync(userId, profile);
            return Ok(ApiResponse.CreateSuccess("Profile updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, ApiResponse.CreateError("Failed to update profile", "UpdateProfileFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-03: Sync Data from LinkedIn
    /// </summary>
    [HttpPost("sync/linkedin")]
    public async Task<IActionResult> SyncLinkedIn([FromBody] SyncLinkedInRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var succeeded = await _profileService.SyncFromLinkedInAsync(userId, request.LinkedInData);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError("Failed to sync LinkedIn data", "SyncLinkedInFailed"));

            return Ok(ApiResponse.CreateSuccess("LinkedIn data synced successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing LinkedIn data");
            return StatusCode(500, ApiResponse.CreateError("Failed to sync LinkedIn data", "SyncLinkedInFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-04: Sync Data from GitHub
    /// </summary>
    [HttpPost("sync/github")]
    public async Task<IActionResult> SyncGitHub([FromBody] SyncGitHubRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var succeeded = await _profileService.SyncFromGitHubAsync(userId, request.GitHubData);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError("Failed to sync GitHub data", "SyncGitHubFailed"));

            return Ok(ApiResponse.CreateSuccess("GitHub data synced successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing GitHub data");
            return StatusCode(500, ApiResponse.CreateError("Failed to sync GitHub data", "SyncGitHubFailed"));
        }
    }
}

public record UpdateProfileRequest(
    string? Bio = null,
    string? Phone = null,
    string? Address = null,
    DateTime? DateOfBirth = null,
    List<string>? SkillIds = null,
    string? Experience = null,
    string? Education = null,
    string? Portfolio = null,
    string? LinkedInUrl = null,
    string? GitHubUrl = null,
    string? Title = null,
    string? Location = null,
    decimal? HourlyRate = null,
    List<string>? Languages = null,
    List<string>? Certifications = null
);

public record SyncLinkedInRequest(string LinkedInData);
public record SyncGitHubRequest(string GitHubData);


