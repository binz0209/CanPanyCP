using System.Text.Json.Serialization;

namespace CanPany.Domain.DTOs.GitHub;

/// <summary>
/// Contributor statistics from GitHub API
/// Based on: https://docs.github.com/en/rest/repos/repos#list-repository-contributors
/// </summary>
public class GitHubContributorStatsDto
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = null!;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = null!;

    [JsonPropertyName("contributions")]
    public int Contributions { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
}

/// <summary>
/// Aggregated contribution statistics for a user across all repositories
/// </summary>
public class GitHubUserContributionSummary
{
    /// <summary>
    /// GitHub username
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Total repositories owned
    /// </summary>
    public int TotalRepositories { get; set; }

    /// <summary>
    /// Total repositories contributed to (including owned)
    /// </summary>
    public int TotalContributedRepositories { get; set; }

    /// <summary>
    /// Total commits/contributions across all repos
    /// </summary>
    public int TotalContributions { get; set; }

    /// <summary>
    /// Total stars received across all repositories
    /// </summary>
    public int TotalStars { get; set; }

    /// <summary>
    /// Total forks across all repositories
    /// </summary>
    public int TotalForks { get; set; }

    /// <summary>
    /// Language usage summary (language -> total bytes)
    /// </summary>
    public Dictionary<string, long> LanguageBytes { get; set; } = new();

    /// <summary>
    /// Repository details
    /// </summary>
    public List<GitHubRepositoryDto> Repositories { get; set; } = new();

    /// <summary>
    /// Get language usage percentage
    /// </summary>
    public Dictionary<string, double> GetLanguagePercentages()
    {
        var total = LanguageBytes.Values.Sum();
        if (total == 0) return new Dictionary<string, double>();

        return LanguageBytes.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round((double)kvp.Value / total * 100, 2)
        );
    }

    /// <summary>
    /// Get top languages by usage
    /// </summary>
    public List<string> GetTopLanguages(int count = 5)
    {
        return LanguageBytes
            .OrderByDescending(kvp => kvp.Value)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
