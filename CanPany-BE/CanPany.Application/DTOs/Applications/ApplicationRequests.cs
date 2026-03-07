namespace CanPany.Application.DTOs.Applications;

public record CreateApplicationRequest(
    string JobId,
    string? CVId = null,
    string? CoverLetter = null,
    decimal? ExpectedSalary = null
);

public record RejectApplicationRequest(string Reason);
public record AddNoteRequest(string Note);
