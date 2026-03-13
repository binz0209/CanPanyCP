using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.DTOs.GitHub;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace CanPany.Infrastructure.Services;

/// <summary>
/// GitHub API service implementation
/// Documentation: https://docs.github.com/en/rest
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubService> _logger;
    private readonly string? _accessToken;
    private const string GitHubApiBaseUrl = "https://api.github.com";

    public GitHubService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _accessToken = configuration["GitHub:AccessToken"];

        // Configure HttpClient for GitHub API
        _httpClient.BaseAddress = new Uri(GitHubApiBaseUrl);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("CanPany", "1.0"));
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        // Add authentication if token is provided
        if (!string.IsNullOrEmpty(_accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }
    }

    public async Task<List<GitHubRepositoryDto>> GetUserRepositoriesAsync(
        string username,
        bool includeForked = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[GITHUB_API] Fetching repositories for user: {Username}",
                username);

            var repositories = new List<GitHubRepositoryDto>();
            var page = 1;
            const int perPage = 100;

            while (true)
            {
                var url = $"/users/{username}/repos?per_page={perPage}&page={page}&sort=updated";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                // Handle 429 rate limiting
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await HandleGitHubRateLimitAsync(response, cancellationToken);
                    continue; // Retry same page
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "[GITHUB_API_ERROR] Failed to fetch repositories. Status: {Status}",
                        response.StatusCode);
                    break;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var pageRepos = JsonSerializer.Deserialize<List<GitHubRepositoryDto>>(json);

                if (pageRepos == null || pageRepos.Count == 0)
                    break;

                repositories.AddRange(pageRepos);

                // Check if there are more pages
                if (pageRepos.Count < perPage)
                    break;

                page++;
            }

            // Filter out forks if requested
            if (!includeForked)
            {
                repositories = repositories.Where(r => !r.IsFork).ToList();
            }

            _logger.LogInformation(
                "[GITHUB_API] Found {Count} repositories for user: {Username}",
                repositories.Count,
                username);

            return repositories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GITHUB_API_ERROR] Error fetching repositories for user: {Username}",
                username);
            return new List<GitHubRepositoryDto>();
        }
    }

    public async Task<GitHubLanguageStatsDto> GetRepositoryLanguagesAsync(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/repos/{owner}/{repo}/languages";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            // Handle 429 rate limiting
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await HandleGitHubRateLimitAsync(response, cancellationToken);
                response = await _httpClient.GetAsync(url, cancellationToken); // Retry once
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[GITHUB_API] Failed to fetch languages for {Owner}/{Repo}. Status: {Status}",
                    owner, repo, response.StatusCode);
                return new GitHubLanguageStatsDto
                {
                    RepositoryFullName = $"{owner}/{repo}"
                };
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            // Check for empty response
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning(
                    "[GITHUB_API] Empty response for languages {Owner}/{Repo}",
                    owner, repo);
                return new GitHubLanguageStatsDto
                {
                    RepositoryFullName = $"{owner}/{repo}"
                };
            }

            // Try to deserialize with error handling
            Dictionary<string, long> languages;
            try
            {
                languages = JsonSerializer.Deserialize<Dictionary<string, long>>(json) ?? new();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx,
                    "[GITHUB_API] Invalid JSON for languages {Owner}/{Repo}. Response: {Response}",
                    owner, repo, json.Length > 200 ? json.Substring(0, 200) + "..." : json);
                languages = new Dictionary<string, long>();
            }

            return new GitHubLanguageStatsDto
            {
                RepositoryFullName = $"{owner}/{repo}",
                Languages = languages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GITHUB_API_ERROR] Error fetching languages for {Owner}/{Repo}",
                owner, repo);
            return new GitHubLanguageStatsDto
            {
                RepositoryFullName = $"{owner}/{repo}"
            };
        }
    }

    public async Task<List<GitHubContributorStatsDto>> GetRepositoryContributorsAsync(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/repos/{owner}/{repo}/contributors?per_page=100";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            // Handle 429 rate limiting
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await HandleGitHubRateLimitAsync(response, cancellationToken);
                response = await _httpClient.GetAsync(url, cancellationToken); // Retry once
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "[GITHUB_API] Failed to fetch contributors for {Owner}/{Repo}. Status: {Status}",
                    owner, repo, response.StatusCode);
                return new List<GitHubContributorStatsDto>();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            // Check for empty response
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning(
                    "[GITHUB_API] Empty response for contributors {Owner}/{Repo}",
                    owner, repo);
                return new List<GitHubContributorStatsDto>();
            }

            // Try to deserialize with error handling
            try
            {
                var contributors = JsonSerializer.Deserialize<List<GitHubContributorStatsDto>>(json);
                return contributors ?? new List<GitHubContributorStatsDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx,
                    "[GITHUB_API] Invalid JSON for contributors {Owner}/{Repo}. Response: {Response}",
                    owner, repo, json.Length > 200 ? json.Substring(0, 200) + "..." : json);
                return new List<GitHubContributorStatsDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GITHUB_API_ERROR] Error fetching contributors for {Owner}/{Repo}",
                owner, repo);
            return new List<GitHubContributorStatsDto>();
        }
    }

    public async Task<GitHubUserContributionSummary> GetUserContributionSummaryAsync(
        string username,
        bool includeForked = false,
        List<string>? selectedRepositories = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "[GITHUB_ANALYSIS] Starting contribution analysis for user: {Username}",
                username);

            var summary = new GitHubUserContributionSummary
            {
                Username = username
            };

            // 1. Fetch all repositories
            var repositories = await GetUserRepositoriesAsync(username, includeForked, cancellationToken);

            // 1b. Filter optionally selected repositories
            if (selectedRepositories != null && selectedRepositories.Count > 0)
            {
                var selectedSet = new HashSet<string>(selectedRepositories, StringComparer.OrdinalIgnoreCase);
                repositories = repositories.Where(r => selectedSet.Contains(r.Name)).ToList();
            }

            summary.Repositories = repositories;
            summary.TotalRepositories = repositories.Count;

            // 2. Calculate basic stats
            summary.TotalStars = repositories.Sum(r => r.StarsCount);
            summary.TotalForks = repositories.Sum(r => r.ForksCount);

            // 3. Aggregate language statistics from all repositories
            var languageAggregation = new Dictionary<string, long>();
            var contributionCount = 0;

            foreach (var repo in repositories)
            {
                // Fetch language stats for each repo
                var langStats = await GetRepositoryLanguagesAsync(
                    repo.Owner.Login,
                    repo.Name,
                    cancellationToken);

                foreach (var (language, bytes) in langStats.Languages)
                {
                    if (languageAggregation.ContainsKey(language))
                        languageAggregation[language] += bytes;
                    else
                        languageAggregation[language] = bytes;
                }

                // Fetch contributor stats to get contribution count
                var contributors = await GetRepositoryContributorsAsync(
                    repo.Owner.Login,
                    repo.Name,
                    cancellationToken);

                var userContribution = contributors.FirstOrDefault(c =>
                    c.Login.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (userContribution != null)
                {
                    contributionCount += userContribution.Contributions;
                }

                // Delay between API calls to avoid rate limiting (GitHub: 60/min unauthenticated, 5000/min authenticated)
                await Task.Delay(200, cancellationToken);
            }

            summary.LanguageBytes = languageAggregation;
            summary.TotalContributions = contributionCount;
            summary.TotalContributedRepositories = repositories.Count;

            _logger.LogInformation(
                "[GITHUB_ANALYSIS] Completed analysis for {Username}. Repos: {Repos}, Languages: {Languages}, Contributions: {Contributions}",
                username,
                summary.TotalRepositories,
                summary.LanguageBytes.Count,
                summary.TotalContributions);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[GITHUB_ANALYSIS_ERROR] Error analyzing contributions for user: {Username}",
                username);
            return new GitHubUserContributionSummary
            {
                Username = username
            };
        }
    }

    /// <summary>
    /// Handle GitHub 429 rate limiting by parsing Retry-After or x-ratelimit-reset headers and waiting.
    /// </summary>
    private async Task HandleGitHubRateLimitAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        int waitSeconds = 60; // Default wait

        // Try Retry-After header
        if (response.Headers.RetryAfter?.Delta != null)
        {
            waitSeconds = (int)response.Headers.RetryAfter.Delta.Value.TotalSeconds;
        }
        // Try x-ratelimit-reset header (Unix timestamp)
        else if (response.Headers.TryGetValues("x-ratelimit-reset", out var resetValues))
        {
            var resetValue = resetValues.FirstOrDefault();
            if (long.TryParse(resetValue, out var resetUnix))
            {
                var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetUnix);
                waitSeconds = Math.Max(1, (int)(resetTime - DateTimeOffset.UtcNow).TotalSeconds);
            }
        }

        // Cap at 5 minutes
        waitSeconds = Math.Min(waitSeconds, 300);

        _logger.LogWarning(
            "[GITHUB_429] Rate limited by GitHub API. Waiting {Seconds}s before retry",
            waitSeconds);

        await Task.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken);
    }
}
