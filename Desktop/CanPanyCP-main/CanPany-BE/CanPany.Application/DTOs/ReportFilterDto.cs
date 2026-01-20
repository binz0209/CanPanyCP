namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for filtering reports (admin)
/// </summary>
public record ReportFilterDto(
    string? Status = null,        // Pending, Resolved, Rejected
    string? ReportType = null,    // User, Company
    DateTime? FromDate = null,
    DateTime? ToDate = null
);
