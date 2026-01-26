namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for creating a new report
/// </summary>
public record CreateReportDto(
    string ReportedUserId,
    string Reason,
    string Description,
    List<string>? Evidence = null
);
