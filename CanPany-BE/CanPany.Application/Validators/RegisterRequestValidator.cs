using CanPany.Application.DTOs.Auth;
using FluentValidation;

namespace CanPany.Application.Validators;

/// <summary>
/// Register request validator
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Role)
            .Must(role => role == "Candidate" || role == "Company" || role == "Admin")
            .WithMessage("Role must be Candidate, Company, or Admin");
    }
}


