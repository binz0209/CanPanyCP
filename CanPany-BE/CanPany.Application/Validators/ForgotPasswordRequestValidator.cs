using CanPany.Application.DTOs.Auth;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Forgot password request validator
/// </summary>
public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
