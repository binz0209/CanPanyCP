using CanPany.Domain.DTOs.GitHub;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// GitHub API service interface
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Get all repositories for a GitHub user
    /// </summary>
    /// <param name="username">GitHub username</param>
    /// <param name="includeForked">Include forked repositories (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of repositories</returns>
    Task<List<GitHubRepositoryDto>> GetUserRepositoriesAsync(
        string username, 
        bool includeForked = false, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get language statistics for a specific repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Language statistics</returns>
    Task<GitHubLanguageStatsDto> GetRepositoryLanguagesAsync(
        string owner, 
        string repo, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get contributors for a specific repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repo">Repository name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of contributors</returns>
    Task<List<GitHubContributorStatsDto>> GetRepositoryContributorsAsync(
        string owner, 
        string repo, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comprehensive contribution summary for a GitHub user
    /// Aggregates repository data, language usage, and contribution statistics
    /// </summary>
    /// <param name="username">GitHub username</param>
    /// <param name="includeForked">Include forked repositories</param>
    /// <param name="selectedRepositories">If provided, only these repositories will be analyzed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User contribution summary</returns>
    Task<GitHubUserContributionSummary> GetUserContributionSummaryAsync(
        string username, 
        bool includeForked = false, 
        List<string>? selectedRepositories = null,
        CancellationToken cancellationToken = default);
}
