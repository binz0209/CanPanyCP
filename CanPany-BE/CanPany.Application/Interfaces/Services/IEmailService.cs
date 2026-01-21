namespace CanPany.Application.Interfaces.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email to a new user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The recipient's name.</param>
    Task SendWelcomeEmailAsync(string toEmail, string userName);

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The recipient's name.</param>
    /// <param name="resetCode">The password reset code.</param>
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetCode);
    Task SendApplicationStatusEmailAsync(string toEmail, string candidateName, string jobTitle, string status);

    /// <summary>
    /// Sends a generic plain text email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="plainTextContent">The plain text content.</param>
    Task SendEmailAsync(string toEmail, string subject, string plainTextContent);
}
