namespace CanPany.Application.DTOs;

/// <summary>
/// CV DTO
/// </summary>
public class CVDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? LatestAnalysisId { get; set; }
    public DateTime CreatedAt { get; set; }
}


