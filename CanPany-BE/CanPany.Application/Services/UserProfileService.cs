using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CanPany.Application.Services;

/// <summary>
/// User profile service implementation
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    private readonly IGitHubAnalysisRepository _githubAnalysisRepo;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserProfileRepository repo,
        IGitHubAnalysisRepository githubAnalysisRepo,
        IGeminiService geminiService,
        ILogger<UserProfileService> logger)
    {
        _repo = repo;
        _githubAnalysisRepo = githubAnalysisRepo;
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
            await Task.CompletedTask;
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
            _logger.LogInformation("[GITHUB_SYNC] Starting sync for user: {UserId}", userId);

            // Parse GitHub data from job result
            var data = JsonSerializer.Deserialize<GitHubSyncData>(gitHubData);
            if (data == null || string.IsNullOrEmpty(data.AnalysisId))
            {
                _logger.LogWarning("[GITHUB_SYNC] Invalid GitHub data for user: {UserId}", userId);
                return false;
            }

            // Retrieve analysis result from RAG storage
            var analysisResult = await _githubAnalysisRepo.GetByIdAsync(data.AnalysisId);
            if (analysisResult == null)
            {
                _logger.LogWarning("[GITHUB_SYNC] Analysis not found: {AnalysisId}", data.AnalysisId);
                return false;
            }

            // Get or create user profile
            var profile = await _repo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            // Update profile with GitHub data
            profile.GitHubUrl = $"https://github.com/{analysisResult.GitHubUsername}";

            // Update skills from AI analysis
            if (analysisResult.SkillAnalysis != null && analysisResult.PrimarySkills.Any())
            {
                // Note: This assumes skill names can be used as IDs
                // In production, you'd need to map skill names to skill IDs from SkillRepository
                profile.SkillIds = analysisResult.PrimarySkills.ToList();
            }

            // Update languages
            var topLanguages = analysisResult.LanguagePercentages
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => kvp.Key)
                .ToList();

            if (topLanguages.Any())
            {
                profile.Languages = topLanguages;
            }

            // Update bio/summary if available
            if (!string.IsNullOrEmpty(analysisResult.AiSummary) && string.IsNullOrEmpty(profile.Bio))
            {
                profile.Bio = analysisResult.AiSummary;
            }

            // Update title based on expertise level
            if (!string.IsNullOrEmpty(analysisResult.ExpertiseLevel) && string.IsNullOrEmpty(profile.Title))
            {
                var specialization = analysisResult.Specializations.FirstOrDefault() ?? "Developer";
                profile.Title = $"{analysisResult.ExpertiseLevel} {specialization}";
            }

            // Update portfolio with top repos
            if (analysisResult.TopRepositories.Any() && string.IsNullOrEmpty(profile.Portfolio))
            {
                var topRepo = analysisResult.TopRepositories.First();
                profile.Portfolio = topRepo.HtmlUrl;
            }

            profile.UpdatedAt = DateTime.UtcNow;

            // Regenerate embedding with new data
            await UpdateEmbeddingAsync(profile);

            // Save profile
            if (string.IsNullOrEmpty(profile.Id))
            {
                await _repo.AddAsync(profile);
            }
            else
            {
                await _repo.UpdateAsync(profile);
            }

            _logger.LogInformation(
                "[GITHUB_SYNC] Successfully synced GitHub data for user: {UserId}. Skills: {Skills}",
                userId,
                string.Join(", ", analysisResult.PrimarySkills));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_SYNC] Error syncing GitHub data: {UserId}", userId);
            return false;
        }
    }
}

/// <summary>
/// GitHub sync data from job completion
/// </summary>
internal record GitHubSyncData(
    string AnalysisId,
    string GitHubUsername,
    string UserId
);


