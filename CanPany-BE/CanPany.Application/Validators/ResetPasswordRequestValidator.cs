using CanPany.Application.DTOs.Auth;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Reset password request validator
/// </summary>
public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset code is required")
            .Length(6).WithMessage("Reset code must be 6 digits")
            .Matches(@"^\d{6}$").WithMessage("Reset code must contain only digits");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
