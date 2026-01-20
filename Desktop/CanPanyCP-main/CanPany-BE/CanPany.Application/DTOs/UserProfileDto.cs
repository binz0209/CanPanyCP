namespace CanPany.Application.DTOs;

/// <summary>
/// User profile DTO
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? HourlyRate { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> SkillIds { get; set; } = new();
}


