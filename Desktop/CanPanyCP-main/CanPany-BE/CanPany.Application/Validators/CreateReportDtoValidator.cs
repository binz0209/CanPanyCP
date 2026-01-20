using CanPany.Application.DTOs;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Validator for CreateReportDto
/// </summary>
public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportedUserId)
            .NotEmpty().WithMessage("Reported user ID is required")
            .NotNull().WithMessage("Reported user ID cannot be null");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MinimumLength(5).WithMessage("Reason must be at least 5 characters")
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Evidence)
            .Must(BeValidUrlList).When(x => x.Evidence != null && x.Evidence.Any())
            .WithMessage("All evidence items must be valid URLs");
    }

    private bool BeValidUrlList(List<string>? urls)
    {
        if (urls == null || !urls.Any())
            return true;

        foreach (var url in urls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                return false;
            }
        }

        return true;
    }
}
