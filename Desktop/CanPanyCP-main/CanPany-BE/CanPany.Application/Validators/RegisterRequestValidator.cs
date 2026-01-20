using CanPany.Application.DTOs.Auth;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CanPany.Application.Validators;

/// <summary>
/// Register request validator with security validations
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    // Allowed characters for full name (letters, spaces, hyphens, apostrophes, Vietnamese characters)
    private static readonly Regex SafeNamePattern = new(@"^[\p{L}\s\-'\.]+$", RegexOptions.Compiled);
    
    // Prevent common injection patterns
    private static readonly Regex InjectionPattern = new(@"[<>""'%;()&+\|\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    // Valid roles - strict whitelist
    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Candidate",
        "Company"
        // Note: Admin role should not be allowed via public registration
    };

    public RegisterRequestValidator()
    {
        // FullName validation - prevent XSS and injection
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters")
            .Must(name => SafeNamePattern.IsMatch(name))
            .WithMessage("Full name contains invalid characters")
            .Must(name => !InjectionPattern.IsMatch(name))
            .WithMessage("Full name contains potentially dangerous characters");

        // Email validation - strict format
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .Must(email => !InjectionPattern.IsMatch(email))
            .WithMessage("Email contains potentially dangerous characters")
            .Must(email => !email.Contains("..") && !email.StartsWith(".") && !email.EndsWith("."))
            .WithMessage("Invalid email format");

        // Password validation - strong password requirements
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
            .Must(password => !password.Contains(" "))
            .WithMessage("Password must not contain spaces")
            .Must(password => !InjectionPattern.IsMatch(password))
            .WithMessage("Password contains potentially dangerous characters");

        // Role validation - strict whitelist (prevent privilege escalation)
        RuleFor(x => x.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));
    }
}


