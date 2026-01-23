namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service for queuing email jobs to be processed asynchronously
/// </summary>
public interface IBackgroundEmailService
{
    /// <summary>
    /// Queue a welcome email to be sent asynchronously
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="userName">User's full name</param>
    /// <returns>Job ID</returns>
    string QueueWelcomeEmail(string email, string userName);

    /// <summary>
    /// Queue a password reset email to be sent asynchronously
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="userName">User's full name</param>
    /// <param name="resetCode">Password reset code</param>
    /// <returns>Job ID</returns>
    string QueuePasswordResetEmail(string email, string userName, string resetCode);

    /// <summary>
    /// Queue an application status email to be sent asynchronously
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="candidateName">Candidate's full name</param>
    /// <param name="jobTitle">Job title</param>
    /// <param name="status">Application status (Accepted/Rejected)</param>
    /// <returns>Job ID</returns>
    string QueueApplicationStatusEmail(string email, string candidateName, string jobTitle, string status);

    /// <summary>
    /// Queue a job match notification email to be sent asynchronously
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="candidateName">Candidate's full name</param>
    /// <param name="jobTitle">Job title</param>
    /// <param name="jobId">Job ID</param>
    /// <param name="companyName">Company name</param>
    /// <param name="location">Job location</param>
    /// <param name="budgetInfo">Budget information</param>
    /// <returns>Job ID</returns>
    string QueueJobMatchEmail(string email, string candidateName, string jobTitle, string jobId, string companyName, string location, string budgetInfo);

    /// <summary>
    /// Queue a payment confirmation email to be sent asynchronously
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="userName">User's full name</param>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Currency</param>
    /// <param name="status">Payment status</param>
    /// <param name="purpose">Payment purpose</param>
    /// <param name="paidAt">Payment date</param>
    /// <returns>Job ID</returns>
    string QueuePaymentConfirmationEmail(string email, string userName, string paymentId, long amount, string currency, string status, string purpose, DateTime paidAt);
}

