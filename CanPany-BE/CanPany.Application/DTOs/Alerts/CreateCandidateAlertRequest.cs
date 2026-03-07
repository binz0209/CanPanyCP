namespace CanPany.Application.DTOs.Alerts;

/// <summary>
/// DTO for creating a new candidate alert
/// </summary>
public record CreateCandidateAlertRequest(
    string Name,
    List<string>? SkillIds = null,
    string? Location = null,
    int? MinExperience = null,
    int? MaxExperience = null,
    string Frequency = "Daily"
);
