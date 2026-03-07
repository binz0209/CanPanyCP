namespace CanPany.Application.DTOs.Alerts;

/// <summary>
/// DTO for creating a new job alert
/// </summary>
public record CreateJobAlertRequest(
    string Name,
    List<string>? SkillIds = null,
    string? CategoryId = null,
    string? Location = null,
    decimal? MinBudget = null,
    decimal? MaxBudget = null,
    bool? IsRemote = null,
    string Frequency = "Daily"
);
