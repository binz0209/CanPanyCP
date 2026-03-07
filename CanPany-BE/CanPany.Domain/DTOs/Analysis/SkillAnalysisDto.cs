namespace CanPany.Domain.DTOs.Analysis;

/// <summary>
/// Skill analysis result from AI (Gemini)
/// </summary>
public class SkillAnalysisDto
{
    /// <summary>
    /// Primary technical skills identified
    /// </summary>
    public List<string> PrimarySkills { get; set; } = new();

    /// <summary>
    /// Overall expertise level assessment
    /// </summary>
    public string ExpertiseLevel { get; set; } = "Mid";

    /// <summary>
    /// Technical specializations identified
    /// </summary>
    public List<string> Specializations { get; set; } = new();

    /// <summary>
    /// Skill proficiency levels
    /// Key: Skill name, Value: Proficiency level
    /// </summary>
    public Dictionary<string, string> SkillProficiency { get; set; } = new();

    /// <summary>
    /// Recommended skills to learn
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// AI-generated summary of developer profile
    /// </summary>
    public string? Summary { get; set; }
}

/// <summary>
/// Proficiency levels for skills
/// </summary>
public enum SkillProficiencyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

/// <summary>
/// Developer expertise level categories
/// </summary>
public enum DeveloperExpertiseLevel
{
    Junior = 1,
    Mid = 2,
    Senior = 3,
    Expert = 4,
    Lead = 5
}

/// <summary>
/// Detailed skill information for user profile
/// </summary>
public class DeveloperSkillDto
{
    /// <summary>
    /// Skill name (e.g., "C#", "TypeScript", "Docker")
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Proficiency level
    /// </summary>
    public SkillProficiencyLevel ProficiencyLevel { get; set; }

    /// <summary>
    /// Years of experience (estimated)
    /// </summary>
    public double? YearsOfExperience { get; set; }

    /// <summary>
    /// Evidence source (e.g., "GitHub", "LinkedIn", "Manual")
    /// </summary>
    public string Source { get; set; } = "GitHub";

    /// <summary>
    /// Supporting data (e.g., % of code, number of projects)
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Complete developer profile analysis result
/// </summary>
public class DeveloperProfileAnalysisDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// GitHub username analyzed
    /// </summary>
    public string? GitHubUsername { get; set; }

    /// <summary>
    /// LinkedIn profile (if available)
    /// </summary>
    public string? LinkedInUrl { get; set; }

    /// <summary>
    /// AI-generated skill analysis
    /// </summary>
    public SkillAnalysisDto? SkillAnalysis { get; set; }

    /// <summary>
    /// Detailed skills breakdown
    /// </summary>
    public List<DeveloperSkillDto> Skills { get; set; } = new();

    /// <summary>
    /// Overall expertise level
    /// </summary>
    public DeveloperExpertiseLevel ExpertiseLevel { get; set; }

    /// <summary>
    /// Technical specializations
    /// </summary>
    public List<string> Specializations { get; set; } = new();

    /// <summary>
    /// Analysis timestamp
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data sources used for analysis
    /// </summary>
    public List<string> DataSources { get; set; } = new();
}
