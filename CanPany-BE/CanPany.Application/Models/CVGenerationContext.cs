namespace CanPany.Application.Models;

/// <summary>
/// Context data used for AI-powered CV generation.
/// Aggregates User + UserProfile data for the Gemini prompt.
/// </summary>
public class CVGenerationContext
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? Experience { get; set; }
    public string? Education { get; set; }
    public string? Portfolio { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();

    // ── Optional: target job context ──────────────────────────────────────
    /// <summary>If set, the CV will be tailored to match this job posting.</summary>
    public string? TargetJobTitle { get; set; }
    public string? TargetJobDescription { get; set; }
    public List<string> TargetJobSkillIds { get; set; } = new();

    /// <summary>Returns true when a target job has been set.</summary>
    public bool HasTargetJob => !string.IsNullOrWhiteSpace(TargetJobTitle) || !string.IsNullOrWhiteSpace(TargetJobDescription);
}
