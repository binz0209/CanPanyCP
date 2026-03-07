using CanPany.Application.DTOs.Alerts;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateJobAlertRequest
/// </summary>
public class CreateJobAlertRequestValidator : AbstractValidator<CreateJobAlertRequest>
{
    private static readonly HashSet<string> ValidFrequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Instant", "Daily", "Weekly"
    };

    public CreateJobAlertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Alert name is required")
            .MaximumLength(100).WithMessage("Alert name must not exceed 100 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.MinBudget)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum budget must be positive")
            .When(x => x.MinBudget.HasValue);

        RuleFor(x => x.MaxBudget)
            .GreaterThanOrEqualTo(x => x.MinBudget ?? 0).WithMessage("Maximum budget must be greater than or equal to minimum budget")
            .When(x => x.MaxBudget.HasValue && x.MinBudget.HasValue);

        RuleFor(x => x.Frequency)
            .Must(f => ValidFrequencies.Contains(f))
            .WithMessage($"Frequency must be one of: {string.Join(", ", ValidFrequencies)}");
    }
}
