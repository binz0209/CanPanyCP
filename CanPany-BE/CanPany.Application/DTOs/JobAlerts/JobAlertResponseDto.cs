namespace CanPany.Application.DTOs.JobAlerts;

public record JobAlertResponseDto(
    string Id,
    string UserId,
    string? Title,
    List<string>? SkillIds,
    List<string>? CategoryIds,
    string? Location,
    string? JobType,
    decimal? MinBudget,
    decimal? MaxBudget,
    string? ExperienceLevel,
    bool IsActive,
    string Frequency,
    bool EmailEnabled,
    bool InAppEnabled,
    DateTime? LastTriggeredAt,
    int MatchCount,
    DateTime CreatedAt);