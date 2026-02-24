namespace CanPany.Application.DTOs.JobAlerts;

public record JobAlertUpdateDto(
    string? Title,
    List<string>? SkillIds,
    List<string>? CategoryIds,
    string? Location,
    string? JobType,
    decimal? MinBudget,
    decimal? MaxBudget,
    string? ExperienceLevel,
    string? Frequency,
    bool? EmailEnabled,
    bool? InAppEnabled);