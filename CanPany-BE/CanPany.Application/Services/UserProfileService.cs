using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// User profile service implementation
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserProfileRepository repo,
        ILogger<UserProfileService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile)
    {
        try
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            profile.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user profile");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string userId, UserProfile profile)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var existing = await _repo.GetByUserIdAsync(userId);
            if (existing == null)
            {
                profile.UserId = userId;
                await _repo.AddAsync(profile);
            }
            else
            {
                profile.Id = existing.Id;
                profile.UserId = userId;
                profile.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(profile);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> SyncFromLinkedInAsync(string userId, string linkedInData)
    {
        try
        {
            // TODO: Parse LinkedIn data and update profile
            // This should parse JSON data from LinkedIn API and update profile fields
            _logger.LogInformation("Syncing LinkedIn data for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing LinkedIn data: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> SyncFromGitHubAsync(string userId, string gitHubData)
    {
        try
        {
            // TODO: Parse GitHub data and update profile
            // This should parse JSON data from GitHub API and update profile fields
            _logger.LogInformation("Syncing GitHub data for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing GitHub data: {UserId}", userId);
            return false;
        }
    }
}


