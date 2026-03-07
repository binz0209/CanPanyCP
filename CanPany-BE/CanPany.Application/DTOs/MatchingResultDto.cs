namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for candidate-job matching result
/// </summary>
public class MatchingResultDto
{
    /// <summary>
    /// Total score (weighted average of breakdowns)
    /// </summary>
    public double TotalMatchScore { get; set; }

    /// <summary>
    /// Breakdown of scores by category
    /// </summary>
    public MatchingBreakdown Breakdown { get; set; } = new();
}

/// <summary>
/// Breakdown of match scores
/// </summary>
public class MatchingBreakdown
{
    /// <summary>
    /// Score based on skill overlap (0-100)
    /// </summary>
    public double SkillMatch { get; set; }

    /// <summary>
    /// Score based on experience relevance (0-100)
    /// </summary>
    public double ExperienceMatch { get; set; }

    /// <summary>
    /// Score based on location and remote preference (0-100)
    /// </summary>
    public double LocationMatch { get; set; }
}
