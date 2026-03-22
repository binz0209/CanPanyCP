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
    private readonly IUserService _userService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(
        IUserProfileService profileService,
        IUserService userService,
        ICloudinaryService cloudinaryService,
        ILogger<UserProfilesController> logger)
    {
        _profileService = profileService;
        _userService = userService;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Upload avatar image
    /// </summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.CreateError("File is required", "FileRequired"));

            // 1. Image Validation
            // Size limit: 2MB
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(ApiResponse.CreateError("Image size exceeds 2MB limit", "FileTooLarge"));

            // Type validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse.CreateError("Only JPG, PNG and WebP images are allowed", "InvalidFileType"));

            // 2. Fetch user to delete old avatar if exists
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse.CreateError("User not found", "UserNotFound"));

            if (!string.IsNullOrWhiteSpace(user.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteAsync(user.CloudinaryPublicId, "image");
            }

            // 3. Upload new image to Cloudinary
            await using var stream = file.OpenReadStream();
            var (secureUrl, publicId) = await _cloudinaryService.UploadAsync(
                stream,
                file.FileName,
                "avatars",
                "image");

            // 4. Update user entity
            user.AvatarUrl = secureUrl;
            user.CloudinaryPublicId = publicId;
            await _userService.UpdateAsync(userId, user);

            return Ok(ApiResponse<object>.CreateSuccess(new { Url = secureUrl, PublicId = publicId }, "Avatar updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, ApiResponse.CreateError("Failed to upload avatar", "UploadAvatarFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-01: View Personal Profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
    /// Delete user profile/account
    /// </summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteProfile()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _userService.DeleteAsync(userId);
            return Ok(ApiResponse.CreateSuccess("Profile deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete profile", "DeleteProfileFailed"));
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
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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


