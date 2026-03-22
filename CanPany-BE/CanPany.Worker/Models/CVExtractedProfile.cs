namespace CanPany.Worker.Models;

/// <summary>
/// DTO for CV profile extraction - contains all profile data extracted from CV
/// </summary>
public class CVExtractedProfile
{
    public ExtractedProfileInfo? Profile { get; set; }
    public string? Experience { get; set; }
    public string? Education { get; set; }
    public ExtractedSkills? ExtractedSkills { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public int? AtsScore { get; set; }
    public ScoreBreakdown? ScoreBreakdown { get; set; }
    public List<string> MissingKeywords { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public string? Summary { get; set; }
}

public class ExtractedProfileInfo
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Portfolio { get; set; }
}

public class ExtractedSkills
{
    public List<string> Technical { get; set; } = new();
    public List<string> Soft { get; set; } = new();
}

public class ScoreBreakdown
{
    public int Keywords { get; set; }
    public int Formatting { get; set; }
    public int Skills { get; set; }
    public int Experience { get; set; }
    public int Education { get; set; }
}
