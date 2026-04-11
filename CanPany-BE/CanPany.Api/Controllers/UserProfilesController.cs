using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanPany.Application.Common.Constants;

namespace CanPany.Api.Controllers;

/// <summary>
/// User Profiles controller - UC-CAN-01, UC-CAN-02, UC-CAN-04
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfilesController : ControllerBase
{
    private readonly IUserProfileService _profileService;
    private readonly IUserService _userService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly II18nService _i18nService;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(
        IUserProfileService profileService,
        IUserService userService,
        ICloudinaryService cloudinaryService,
        II18nService i18nService,
        ILogger<UserProfilesController> logger)
    {
        _profileService = profileService;
        _userService = userService;
        _cloudinaryService = cloudinaryService;
        _i18nService = i18nService;
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
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.FileRequired);
                return BadRequest(ApiResponse.CreateError(errorMsg, "FileRequired"));
            }

            // 1. Image Validation
            // Size limit: 2MB
            if (file.Length > 2 * 1024 * 1024)
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.FileTooLarge);
                return BadRequest(ApiResponse.CreateError(errorMsg, "FileTooLarge"));
            }

            // Type validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.InvalidFileType);
                return BadRequest(ApiResponse.CreateError(errorMsg, "InvalidFileType"));
            }

            // 2. Fetch user to delete old avatar if exists
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.User.NotFound);
                return NotFound(ApiResponse.CreateError(errorMsg, "UserNotFound"));
            }

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

            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.AvatarUpdated);
            return Ok(ApiResponse<object>.CreateSuccess(new { Url = secureUrl, PublicId = publicId }, successMsg));
        }
        catch (Exception ex)
        {
            var errorMsg = _i18nService.GetLogMessage(I18nKeys.Error.Profile.Avatar.UploadFailed);
            _logger.LogError(ex, errorMsg);
            
            var userMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.UploadFailed);
            return StatusCode(500, ApiResponse.CreateError(userMsg, "UploadAvatarFailed"));
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
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.NotFound);
                return NotFound(ApiResponse.CreateError(errorMsg, "NotFound"));
            }

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

            if (!string.IsNullOrWhiteSpace(request.GitHubUrl)) profile.GitHubUrl = request.GitHubUrl;
            if (!string.IsNullOrWhiteSpace(request.Title)) profile.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Location)) profile.Location = request.Location;
            if (request.HourlyRate.HasValue) profile.HourlyRate = request.HourlyRate;
            if (request.Languages != null) profile.Languages = request.Languages;
            if (request.Certifications != null) profile.Certifications = request.Certifications;

            await _profileService.UpdateAsync(userId, profile);
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Deleted);
            return Ok(ApiResponse.CreateSuccess(successMsg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete profile", "DeleteProfileFailed"));
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
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.SyncGitHubFailed);
                return BadRequest(ApiResponse.CreateError(errorMsg, "SyncGitHubFailed"));
            }

            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.GitHubSynced);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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

    string? GitHubUrl = null,
    string? Title = null,
    string? Location = null,
    decimal? HourlyRate = null,
    List<string>? Languages = null,
    List<string>? Certifications = null
);


public record SyncGitHubRequest(string GitHubData);


