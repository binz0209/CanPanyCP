using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UserProfileService> _logger;
    private readonly string _encryptionKey;

    public UserProfileService(
        IUserProfileRepository repo,
        IGitHubAnalysisRepository githubAnalysisRepo,
        IGeminiService geminiService,
        IEncryptionService encryptionService,
        IConfiguration configuration,
        ILogger<UserProfileService> logger)
    {
        _repo = repo;
        _githubAnalysisRepo = githubAnalysisRepo;
        _geminiService = geminiService;
        _encryptionService = encryptionService;
        _logger = logger;
        _encryptionKey = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var profile = await _repo.GetByUserIdAsync(userId);
            if (profile != null)
                DecryptPII(profile);
            return profile;
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
            EncryptPII(profile);
            var saved = await _repo.AddAsync(profile);
            DecryptPII(saved);
            return saved;
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
                EncryptPII(profile);
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
                EncryptPII(profile);
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

    // ==================== GitHub OAuth Linking ====================

    public async Task<bool> LinkGitHubAsync(string userId, string gitHubUsername, string gitHubUrl)
    {
        try
        {
            var profile = await _repo.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    GitHubUrl = gitHubUrl,
                    CreatedAt = DateTime.UtcNow
                };
                EncryptPII(profile);
                await _repo.AddAsync(profile);
            }
            else
            {
                profile.GitHubUrl = gitHubUrl;
                profile.UpdatedAt = DateTime.UtcNow;
                EncryptPII(profile);
                await _repo.UpdateAsync(profile);
            }

            _logger.LogInformation(
                "[GITHUB_LINK] Linked GitHub account '{Username}' for user {UserId}",
                gitHubUsername, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_LINK] Failed to link GitHub account for user {UserId}", userId);
            return false;
        }
    }

    // ==================== PII Encryption Helpers ====================

    private void EncryptPII(UserProfile profile)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(profile.Phone))
                profile.Phone = _encryptionService.Encrypt(profile.Phone, _encryptionKey);
            if (!string.IsNullOrWhiteSpace(profile.Address))
                profile.Address = _encryptionService.Encrypt(profile.Address, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt PII for user profile");
            // Don't throw - allow save even if encryption fails
        }
    }

    private void DecryptPII(UserProfile profile)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(profile.Phone))
                profile.Phone = _encryptionService.Decrypt(profile.Phone, _encryptionKey);
            if (!string.IsNullOrWhiteSpace(profile.Address))
                profile.Address = _encryptionService.Decrypt(profile.Address, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt PII for user profile (may be unencrypted legacy data)");
            // Don't throw - return raw data if decryption fails (legacy data)
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


