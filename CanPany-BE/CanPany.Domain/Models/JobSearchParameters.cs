namespace CanPany.Domain.Models;

public class JobSearchParameters
{
    public string? Keyword { get; set; }
    public string? CategoryId { get; set; }
    public List<string>? SkillIds { get; set; }
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public string? Level { get; set; }
    public string? Location { get; set; }
    public string? BudgetType { get; set; }
    public bool? IsRemote { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
