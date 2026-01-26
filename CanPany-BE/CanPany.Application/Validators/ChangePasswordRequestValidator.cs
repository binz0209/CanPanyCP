using CanPany.Application.DTOs.Auth;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Change password request validator
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .NotEqual(x => x.OldPassword).WithMessage("New password must be different from old password");
    }
}
