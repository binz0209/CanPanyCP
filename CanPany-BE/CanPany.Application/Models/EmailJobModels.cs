namespace CanPany.Application.Models;

/// <summary>
/// Job payload for sending welcome email
/// </summary>
public record WelcomeEmailJob
{
    public required string Email { get; init; }
    public required string UserName { get; init; }
}

/// <summary>
/// Job payload for sending password reset email
/// </summary>
public record PasswordResetEmailJob
{
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public required string ResetCode { get; init; }
}

/// <summary>
/// Job payload for sending application status email
/// </summary>
public record ApplicationStatusEmailJob
{
    public required string Email { get; init; }
    public required string CandidateName { get; init; }
    public required string JobTitle { get; init; }
    public required string Status { get; init; }
}
