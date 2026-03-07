namespace CanPany.Application.DTOs.Jobs;

public record CreateJobRequest(
    string CompanyId,
    string Title,
    string Description,
    string? CategoryId = null,
    List<string>? SkillIds = null,
    string BudgetType = "Fixed",
    decimal? BudgetAmount = null,
    string? Level = null,
    string? Location = null,
    bool IsRemote = false,
    DateTime? Deadline = null
);

public record UpdateJobRequest(
    string? Title = null,
    string? Description = null,
    List<string>? SkillIds = null,
    decimal? BudgetAmount = null,
    string? Level = null,
    string? Location = null,
    DateTime? Deadline = null
);
