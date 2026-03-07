using CanPany.Application.DTOs.Alerts;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateCandidateAlertRequest
/// </summary>
public class CreateCandidateAlertRequestValidator : AbstractValidator<CreateCandidateAlertRequest>
{
    private static readonly HashSet<string> ValidFrequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Instant", "Daily", "Weekly"
    };

    public CreateCandidateAlertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Alert name is required")
            .MaximumLength(100).WithMessage("Alert name must not exceed 100 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.MinExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum experience must be positive")
            .When(x => x.MinExperience.HasValue);

        RuleFor(x => x.MaxExperience)
            .GreaterThanOrEqualTo(x => x.MinExperience ?? 0).WithMessage("Maximum experience must be greater than or equal to minimum experience")
            .When(x => x.MaxExperience.HasValue && x.MinExperience.HasValue);

        RuleFor(x => x.Frequency)
            .Must(f => ValidFrequencies.Contains(f))
            .WithMessage($"Frequency must be one of: {string.Join(", ", ValidFrequencies)}");
    }
}
