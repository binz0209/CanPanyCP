using System.Text.Json.Serialization;

namespace CanPany.Domain.DTOs.GitHub;

/// <summary>
/// GitHub repository information from GitHub API
/// Based on: https://docs.github.com/en/rest/repos/repos#list-repositories-for-a-user
/// </summary>
public class GitHubRepositoryDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = null!;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StarsCount { get; set; }

    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }

    [JsonPropertyName("watchers_count")]
    public int WatchersCount { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("pushed_at")]
    public DateTime? PushedAt { get; set; }

    [JsonPropertyName("fork")]
    public bool IsFork { get; set; }

    [JsonPropertyName("archived")]
    public bool IsArchived { get; set; }

    [JsonPropertyName("owner")]
    public GitHubOwnerDto Owner { get; set; } = null!;

    [JsonPropertyName("languages_url")]
    public string LanguagesUrl { get; set; } = null!;

    [JsonPropertyName("contributors_url")]
    public string ContributorsUrl { get; set; } = null!;
}

/// <summary>
/// Repository owner information
/// </summary>
public class GitHubOwnerDto
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = null!;

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = null!;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;
}
