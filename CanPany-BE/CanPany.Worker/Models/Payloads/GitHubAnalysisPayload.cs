namespace CanPany.Worker.Models.Payloads;

/// <summary>
/// Payload for GitHub repository analysis job
/// </summary>
public record GitHubAnalysisPayload
{
    /// <summary>
    /// GitHub username to analyze
    /// </summary>
    public string GitHubUsername { get; init; } = null!;

    /// <summary>
    /// User ID in our system (to update profile after analysis)
    /// </summary>
    public string UserId { get; init; } = null!;

    /// <summary>
    /// Include forked repositories in analysis
    /// </summary>
    public bool IncludeForkedRepos { get; init; } = false;

    /// <summary>
    /// Analyze and extract skills using Gemini AI
    /// </summary>
    public bool AnalyzeSkills { get; init; } = true;

    /// <summary>
    /// Update user profile with analyzed data
    /// </summary>
    public bool UpdateProfile { get; init; } = true;

    /// <summary>
    /// Specific repository names to analyze (null/empty = analyze all)
    /// </summary>
    public List<string>? SelectedRepositories { get; init; }
}
