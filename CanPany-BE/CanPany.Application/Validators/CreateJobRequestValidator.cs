using CanPany.Application.DTOs.Jobs;
using CanPany.Domain.Enums;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateJobRequest
/// </summary>
public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required")
            .MaximumLength(150).WithMessage("Job title must not exceed 150 characters")
            .MinimumLength(5).WithMessage("Job title must be at least 5 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Job description is required")
            .MinimumLength(20).WithMessage("Job description must be at least 20 characters");

        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0).WithMessage("Budget must be greater than 0")
            .When(x => x.BudgetAmount.HasValue);

        RuleFor(x => x.SkillIds)
            .NotEmpty().WithMessage("At least one required skill must be selected");

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow).WithMessage("Deadline must be in the future")
            .When(x => x.Deadline.HasValue);

        RuleFor(x => x.Level)
            .Must(level => string.IsNullOrEmpty(level) || Enum.IsDefined(typeof(JobLevel), level))
            .WithMessage("Invalid job level");
        
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required");
    }
}
