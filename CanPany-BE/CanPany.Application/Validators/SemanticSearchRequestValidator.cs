using CanPany.Application.DTOs;
using FluentValidation;

namespace CanPany.Application.Validators;

public class SemanticSearchRequestValidator : AbstractValidator<SemanticSearchRequest>
{
    public SemanticSearchRequestValidator()
    {
        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("Job description is required")
            .MinimumLength(20).WithMessage("Job description must be at least 20 characters long");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");
    }
}
