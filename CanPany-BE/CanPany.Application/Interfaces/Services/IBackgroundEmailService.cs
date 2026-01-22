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
}
