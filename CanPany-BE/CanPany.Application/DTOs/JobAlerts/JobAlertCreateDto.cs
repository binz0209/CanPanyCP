namespace CanPany.Application.DTOs.JobAlerts;

public record JobAlertCreateDto(
    string? Title,
    List<string>? SkillIds,
    List<string>? CategoryIds,
    string? Location,
    string? JobType,
    decimal? MinBudget,
    decimal? MaxBudget,
    string? ExperienceLevel,
    string Frequency = "Daily",
    bool EmailEnabled = true,
    bool InAppEnabled = true);