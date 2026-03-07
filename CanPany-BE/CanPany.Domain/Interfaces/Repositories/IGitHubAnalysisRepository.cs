using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for GitHub Analysis Results (RAG Layer)
/// </summary>
public interface IGitHubAnalysisRepository
{
    /// <summary>
    /// Get analysis result by ID
    /// </summary>
    Task<GitHubAnalysisResult?> GetByIdAsync(string id);

    /// <summary>
    /// Get latest active analysis for a user
    /// </summary>
    Task<GitHubAnalysisResult?> GetLatestByUserIdAsync(string userId);

    /// <summary>
    /// Get all analysis history for a user
    /// </summary>
    Task<List<GitHubAnalysisResult>> GetByUserIdAsync(string userId, int limit = 10);

    /// <summary>
    /// Get analysis by GitHub username
    /// </summary>
    Task<GitHubAnalysisResult?> GetByGitHubUsernameAsync(string gitHubUsername);

    /// <summary>
    /// Add new analysis result
    /// </summary>
    Task<GitHubAnalysisResult> AddAsync(GitHubAnalysisResult analysisResult);

    /// <summary>
    /// Update existing analysis result
    /// </summary>
    Task<bool> UpdateAsync(GitHubAnalysisResult analysisResult);

    /// <summary>
    /// Deactivate old analyses and set new one as active
    /// </summary>
    Task<bool> SetActiveAnalysisAsync(string userId, string newAnalysisId);

    /// <summary>
    /// Delete analysis result
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Get analyses by skill
    /// </summary>
    Task<List<GitHubAnalysisResult>> GetBySkillAsync(string skill, int limit = 50);
}
