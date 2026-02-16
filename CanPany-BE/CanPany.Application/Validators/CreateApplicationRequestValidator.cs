using CanPany.Application.DTOs.Applications;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateApplicationRequest
/// </summary>
public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage("Job ID is required");

        RuleFor(x => x.CVId)
            .NotEmpty().WithMessage("CV must be selected or uploaded");

        RuleFor(x => x.CoverLetter)
            .MaximumLength(2000).WithMessage("Cover letter must not exceed 2000 characters");

        RuleFor(x => x.ExpectedSalary)
            .GreaterThan(0).WithMessage("Expected salary must be greater than 0")
            .When(x => x.ExpectedSalary.HasValue);
    }
}
