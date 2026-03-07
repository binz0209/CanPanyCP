namespace CanPany.Domain.DTOs.GitHub;

/// <summary>
/// Language usage statistics for a repository
/// Based on: https://docs.github.com/en/rest/repos/repos#list-repository-languages
/// </summary>
public class GitHubLanguageStatsDto
{
    /// <summary>
    /// Repository full name (owner/repo)
    /// </summary>
    public string RepositoryFullName { get; set; } = null!;

    /// <summary>
    /// Language name and bytes of code
    /// Key: Language name (e.g., "C#", "TypeScript")
    /// Value: Bytes of code in that language
    /// </summary>
    public Dictionary<string, long> Languages { get; set; } = new();

    /// <summary>
    /// Total bytes across all languages
    /// </summary>
    public long TotalBytes => Languages.Values.Sum();

    /// <summary>
    /// Get language percentage breakdown
    /// </summary>
    public Dictionary<string, double> GetLanguagePercentages()
    {
        if (TotalBytes == 0) return new Dictionary<string, double>();

        return Languages.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round((double)kvp.Value / TotalBytes * 100, 2)
        );
    }

    /// <summary>
    /// Get dominant language (most used)
    /// </summary>
    public string? GetDominantLanguage()
    {
        return Languages
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault()
            .Key;
    }
}
