namespace CanPany.Application.DTOs;

/// <summary>
/// Job DTO
/// </summary>
public class JobDto
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public List<string> SkillIds { get; set; } = new();
    public string BudgetType { get; set; } = "Fixed";
    public decimal? BudgetAmount { get; set; }
    public string? Level { get; set; }
    public string? Location { get; set; }
    public bool IsRemote { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; }
}


