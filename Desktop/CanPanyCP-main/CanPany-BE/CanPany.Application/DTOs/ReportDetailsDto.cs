namespace CanPany.Application.DTOs;

/// <summary>
/// DTO for report details with populated user information
/// </summary>
public record ReportDetailsDto(
    string Id,
    UserBasicInfo Reporter,
    UserBasicInfo? ReportedUser,
    string Reason,
    string Description,
    List<string>? Evidence,
    string Status,
    string? ResolutionNote,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);

/// <summary>
/// Basic user information for reports
/// </summary>
public record UserBasicInfo(
    string Id,
    string FullName,
    string Email,
    string? AvatarUrl
);
