using CanPany.Application.DTOs;
using FluentValidation;

namespace CanPany.Application.Validators;

public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportedUserId)
            .NotEmpty().WithMessage("Reported user ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(100).WithMessage("Reason cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long");
    }
}
