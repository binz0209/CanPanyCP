using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Jobs;

/// <summary>
/// Background job processor for matching jobs against job alerts
/// </summary>
public class JobMatchProcessor
{
    private readonly IJobAlertService _jobAlertService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IBackgroundEmailService _backgroundEmailService;
    private readonly INotificationService _notificationService;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobMatchProcessor> _logger;

    public JobMatchProcessor(
        IJobAlertService jobAlertService,
        IUserService userService,
        ICompanyService companyService,
        IBackgroundEmailService backgroundEmailService,
        INotificationService notificationService,
        IJobRepository jobRepository,
        ILogger<JobMatchProcessor> logger)
    {
        _jobAlertService = jobAlertService;
        _userService = userService;
        _companyService = companyService;
        _backgroundEmailService = backgroundEmailService;
        _notificationService = notificationService;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    /// <summary>
    /// Process job alerts for a newly created job
    /// </summary>
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
    public async Task ProcessJobAlertsAsync(string jobId, string jobTitle, string companyId, string? location, decimal? budgetAmount, string budgetType)
    {
        try
        {
            _logger.LogInformation("Processing job alerts for job {JobId}: {JobTitle}", jobId, jobTitle);

            // Get the job details (we'll need to fetch it from repository)
            // For now, we'll work with the parameters passed
            
            var company = await _companyService.GetByIdAsync(companyId);
            if (company == null)
            {
                _logger.LogWarning("Company not found for job {JobId}", jobId);
                return;
            }

            var companyName = company.Name ?? "Unknown Company";
            var locationText = location ?? "Remote/Flexible";
            var budgetInfo = budgetAmount.HasValue 
                ? $"{budgetAmount:N0} VND ({budgetType})" 
                : "Negotiable";

            // This is a simplified version - in production, you'd fetch the full job
            // and use the JobAlertService.FindMatchingAlertsAsync method
            
            _logger.LogInformation(
                "Job alert processing completed for job {JobId}. Company: {CompanyName}",
                jobId,
                companyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job alerts for job {JobId}", jobId);
            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Process job alerts for a newly created job (full job object)
    /// </summary>
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
    public async Task ProcessJobAlertsForJobAsync(string jobId)
    {
        try
        {
            _logger.LogInformation("Processing job alerts for job {JobId}", jobId);

            // Fetch the job from repository
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning("Job not found: {JobId}", jobId);
                return;
            }

            _logger.LogInformation("Found job {JobId}: {JobTitle}", job.Id, job.Title);

            // Find all matching job alerts
            var matchingAlerts = await _jobAlertService.FindMatchingAlertsAsync(job);

            if (!matchingAlerts.Any())
            {
                _logger.LogInformation("No matching alerts found for job {JobId}", job.Id);
                return;
            }

            _logger.LogInformation("Found {Count} matching alerts for job {JobId}", matchingAlerts.Count(), job.Id);

            // Get company details
            var company = await _companyService.GetByIdAsync(job.CompanyId);
            var companyName = company?.Name ?? "Unknown Company";
            var locationText = job.Location ?? (job.IsRemote ? "Remote" : "Not specified");
            var budgetInfo = job.BudgetAmount.HasValue 
                ? $"{job.BudgetAmount:N0} VND ({job.BudgetType})" 
                : "Negotiable";

            // Send notifications to all matching candidates
            foreach (var alert in matchingAlerts)
            {
                try
                {
                    var user = await _userService.GetByIdAsync(alert.UserId);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found for alert {AlertId}", alert.Id);
                        continue;
                    }

                    // Queue email notification
                    _backgroundEmailService.QueueJobMatchEmail(
                        user.Email,
                        user.FullName,
                        job.Title,
                        job.Id,
                        companyName,
                        locationText,
                        budgetInfo);

                    // Create in-app notification
                    var notification = new Notification
                    {
                        UserId = alert.UserId,
                        Type = "JobMatch",
                        Title = "New Job Match Found!",
                        Message = $"A new job '{job.Title}' at {companyName} matches your alert '{alert.Name}'.",
                        Payload = System.Text.Json.JsonSerializer.Serialize(new { JobId = job.Id, AlertId = alert.Id }),
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await _notificationService.CreateAsync(notification);

                    _logger.LogInformation(
                        "Sent job match notification to user {UserId} for job {JobId}",
                        alert.UserId,
                        job.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send notification for alert {AlertId}, user {UserId}",
                        alert.Id,
                        alert.UserId);
                    // Continue with other alerts even if one fails
                }
            }

            _logger.LogInformation(
                "Job alert processing completed for job {JobId}. Notified {Count} candidates.",
                job.Id,
                matchingAlerts.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job alerts for job {JobId}", jobId);
            throw; // Re-throw to trigger Hangfire retry
        }
    }
}

