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
    private readonly IGeminiService _geminiService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserProfileRepository repo,
        IGeminiService geminiService,
        ILogger<UserProfileService> logger)
    {
        _repo = repo;
        _geminiService = geminiService;
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
            await UpdateEmbeddingAsync(profile);
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
                await UpdateEmbeddingAsync(profile);
                await _repo.AddAsync(profile);
            }
            else
            {
                // Preserve ID and CreatedAt
                profile.Id = existing.Id;
                profile.UserId = userId;
                profile.CreatedAt = existing.CreatedAt;
                profile.UpdatedAt = DateTime.UtcNow;
                
                await UpdateEmbeddingAsync(profile);
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

    private async Task UpdateEmbeddingAsync(UserProfile profile)
    {
        try
        {
            var textParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(profile.Title)) textParts.Add(profile.Title);
            if (!string.IsNullOrWhiteSpace(profile.Bio)) textParts.Add(profile.Bio);
            if (!string.IsNullOrWhiteSpace(profile.Experience)) textParts.Add(profile.Experience);
            if (!string.IsNullOrWhiteSpace(profile.Education)) textParts.Add(profile.Education);
            if (profile.SkillIds != null && profile.SkillIds.Any()) textParts.Add(string.Join(" ", profile.SkillIds));

            var text = string.Join(" ", textParts);
            if (!string.IsNullOrWhiteSpace(text))
            {
                profile.Embedding = await _geminiService.GenerateEmbeddingAsync(text);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for user profile");
            // Don't throw, allow profile update even if embedding fails
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


