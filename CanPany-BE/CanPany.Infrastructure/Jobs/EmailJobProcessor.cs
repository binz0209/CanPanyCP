using CanPany.Application.Interfaces.Services;
using CanPany.Application.Models;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Jobs;

/// <summary>
/// Processes email jobs queued by BackgroundEmailService
/// </summary>
public class EmailJobProcessor
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailJobProcessor> _logger;

    public EmailJobProcessor(
        IEmailService emailService,
        ILogger<EmailJobProcessor> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Process welcome email job
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessWelcomeEmail(WelcomeEmailJob job)
    {
        try
        {
            _logger.LogInformation(
                "Processing welcome email job for {Email}",
                job.Email);

            await _emailService.SendWelcomeEmailAsync(job.Email, job.UserName);

            _logger.LogInformation(
                "Successfully sent welcome email to {Email}",
                job.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email to {Email}",
                job.Email);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Process password reset email job
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessPasswordResetEmail(PasswordResetEmailJob job)
    {
        try
        {
            _logger.LogInformation(
                "Processing password reset email job for {Email}",
                job.Email);

            await _emailService.SendPasswordResetEmailAsync(
                job.Email,
                job.UserName,
                job.ResetCode);

            _logger.LogInformation(
                "Successfully sent password reset email to {Email}",
                job.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send password reset email to {Email}",
                job.Email);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Process application status email job
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessApplicationStatusEmail(ApplicationStatusEmailJob job)
    {
        try
        {
            _logger.LogInformation(
                "Processing application status email job for {Email} - Status: {Status}",
                job.Email,
                job.Status);

            await _emailService.SendApplicationStatusEmailAsync(
                job.Email,
                job.CandidateName,
                job.JobTitle,
                job.Status);

            _logger.LogInformation(
                "Successfully sent application status email to {Email} - Status: {Status}",
                job.Email,
                job.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send application status email to {Email} - Status: {Status}",
                job.Email,
                job.Status);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Process job match notification email job
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessJobMatchEmail(JobMatchEmailJob job)
    {
        try
        {
            _logger.LogInformation(
                "Processing job match email job for {Email} - Job: {JobTitle}",
                job.Email,
                job.JobTitle);

            await _emailService.SendJobMatchEmailAsync(
                job.Email,
                job.CandidateName,
                job.JobTitle,
                job.JobId,
                job.CompanyName,
                job.Location,
                job.BudgetInfo);

            _logger.LogInformation(
                "Successfully sent job match email to {Email} - Job: {JobTitle}",
                job.Email,
                job.JobTitle);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send job match email to {Email} - Job: {JobTitle}",
                job.Email,
                job.JobTitle);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Process payment confirmation email job
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
    public async Task ProcessPaymentConfirmationEmail(PaymentConfirmationEmailJob job)
    {
        try
        {
            _logger.LogInformation(
                "Processing payment confirmation email job for {Email} - Status: {Status}",
                job.Email,
                job.Status);

            await _emailService.SendPaymentConfirmationEmailAsync(
                job.Email,
                job.UserName,
                job.PaymentId,
                job.Amount,
                job.Currency,
                job.Status,
                job.Purpose,
                job.PaidAt);

            _logger.LogInformation(
                "Successfully sent payment confirmation email to {Email} - Status: {Status}",
                job.Email,
                job.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send payment confirmation email to {Email} - Status: {Status}",
                job.Email,
                job.Status);
            throw; // Re-throw to trigger Hangfire retry
        }
    }
}

