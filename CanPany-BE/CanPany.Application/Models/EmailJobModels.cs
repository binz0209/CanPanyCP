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

/// <summary>
/// Job payload for sending job match notification email
/// </summary>
public record JobMatchEmailJob
{
    public required string Email { get; init; }
    public required string CandidateName { get; init; }
    public required string JobTitle { get; init; }
    public required string JobId { get; init; }
    public required string CompanyName { get; init; }
    public required string Location { get; init; }
    public required string BudgetInfo { get; init; }
}

/// <summary>
/// Job payload for sending payment confirmation email
/// </summary>
public record PaymentConfirmationEmailJob
{
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public required string PaymentId { get; init; }
    public required long Amount { get; init; }
    public required string Currency { get; init; }
    public required string Status { get; init; }
    public required string Purpose { get; init; }
    public required DateTime PaidAt { get; init; }
}

