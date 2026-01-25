using CanPany.Application.Interfaces.Services;
using CanPany.Application.Models;
using CanPany.Infrastructure.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// Background email service implementation using Hangfire
/// </summary>
public class BackgroundEmailService : IBackgroundEmailService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<BackgroundEmailService> _logger;

    public BackgroundEmailService(
        IBackgroundJobClient backgroundJobClient,
        ILogger<BackgroundEmailService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public string QueueWelcomeEmail(string email, string userName)
    {
        var job = new WelcomeEmailJob
        {
            Email = email,
            UserName = userName
        };

        var jobId = _backgroundJobClient.Enqueue<EmailJobProcessor>(
            processor => processor.ProcessWelcomeEmail(job));

        _logger.LogInformation(
            "Queued welcome email job {JobId} for {Email}",
            jobId,
            email);

        return jobId;
    }

    public string QueuePasswordResetEmail(string email, string userName, string resetCode)
    {
        var job = new PasswordResetEmailJob
        {
            Email = email,
            UserName = userName,
            ResetCode = resetCode
        };

        var jobId = _backgroundJobClient.Enqueue<EmailJobProcessor>(
            processor => processor.ProcessPasswordResetEmail(job));

        _logger.LogInformation(
            "Queued password reset email job {JobId} for {Email}",
            jobId,
            email);

        return jobId;
    }

    public string QueueApplicationStatusEmail(string email, string candidateName, string jobTitle, string status)
    {
        var job = new ApplicationStatusEmailJob
        {
            Email = email,
            CandidateName = candidateName,
            JobTitle = jobTitle,
            Status = status
        };

        var jobId = _backgroundJobClient.Enqueue<EmailJobProcessor>(
            processor => processor.ProcessApplicationStatusEmail(job));

        _logger.LogInformation(
            "Queued application status email job {JobId} for {Email} - Status: {Status}",
            jobId,
            email,
            status);

        return jobId;
    }

    public string QueueJobMatchEmail(string email, string candidateName, string jobTitle, string jobId, string companyName, string location, string budgetInfo)
    {
        var job = new JobMatchEmailJob
        {
            Email = email,
            CandidateName = candidateName,
            JobTitle = jobTitle,
            JobId = jobId,
            CompanyName = companyName,
            Location = location,
            BudgetInfo = budgetInfo
        };

        var hangfireJobId = _backgroundJobClient.Enqueue<EmailJobProcessor>(
            processor => processor.ProcessJobMatchEmail(job));

        _logger.LogInformation(
            "Queued job match email job {JobId} for {Email} - Job: {JobTitle}",
            hangfireJobId,
            email,
            jobTitle);

        return hangfireJobId;
    }

    public string QueuePaymentConfirmationEmail(string email, string userName, string paymentId, long amount, string currency, string status, string purpose, DateTime paidAt)
    {
        var job = new PaymentConfirmationEmailJob
        {
            Email = email,
            UserName = userName,
            PaymentId = paymentId,
            Amount = amount,
            Currency = currency,
            Status = status,
            Purpose = purpose,
            PaidAt = paidAt
        };

        var hangfireJobId = _backgroundJobClient.Enqueue<EmailJobProcessor>(
            processor => processor.ProcessPaymentConfirmationEmail(job));

        _logger.LogInformation(
            "Queued payment confirmation email job {JobId} for {Email} - Status: {Status}",
            hangfireJobId,
            email,
            status);

        return hangfireJobId;
    }
}

