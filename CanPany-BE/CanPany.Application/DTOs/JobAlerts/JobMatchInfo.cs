namespace CanPany.Application.DTOs.JobAlerts;

public record JobMatchInfo(
    string JobId,
    string JobTitle,
    string CompanyName,
    string Location,
    string Budget,
    int MatchScore);