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
    
    /// <summary>
    /// Sends an application status email.
    /// </summary>
    Task SendApplicationStatusEmailAsync(string toEmail, string candidateName, string jobTitle, string status);

    /// <summary>
    /// Sends a job match notification email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="candidateName">The candidate's name.</param>
    /// <param name="jobTitle">The job title.</param>
    /// <param name="jobId">The job ID.</param>
    /// <param name="companyName">The company name.</param>
    /// <param name="location">The job location.</param>
    /// <param name="budgetInfo">Budget information.</param>
    Task SendJobMatchEmailAsync(string toEmail, string candidateName, string jobTitle, string jobId, string companyName, string location, string budgetInfo);

    /// <summary>
    /// Sends a payment confirmation email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The user's name.</param>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="amount">The payment amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="status">The payment status.</param>
    /// <param name="purpose">The payment purpose.</param>
    /// <param name="paidAt">The payment date.</param>
    Task SendPaymentConfirmationEmailAsync(string toEmail, string userName, string paymentId, long amount, string currency, string status, string purpose, DateTime paidAt);

    /// <summary>
    /// Sends a generic plain text email.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="plainTextContent">The plain text content.</param>
    Task SendEmailAsync(string toEmail, string subject, string plainTextContent);
}
