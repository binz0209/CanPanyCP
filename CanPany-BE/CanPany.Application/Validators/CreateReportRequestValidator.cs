using CanPany.Application.DTOs;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateReportDto
/// </summary>
public class CreateReportRequestValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportRequestValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.ReportedUserId) ||
                !string.IsNullOrWhiteSpace(x.ReportedCompanyId) ||
                !string.IsNullOrWhiteSpace(x.ReportedJobId))
            .WithMessage("At least one report target is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MinimumLength(5).WithMessage("Reason must be at least 5 characters")
            .MaximumLength(100).WithMessage("Reason must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}
