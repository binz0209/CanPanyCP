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
}
