using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Jobs;

/// <summary>
/// Background job processor for job alerts
/// </summary>
public class JobAlertProcessor
{
    private readonly IJobAlertRepository _alertRepo;
    private readonly IJobAlertMatchRepository _matchRepo;
    private readonly IJobRepository _jobRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IJobAlertService _jobAlertService;
    private readonly ILogger<JobAlertProcessor> _logger;

    public JobAlertProcessor(
        IJobAlertRepository alertRepo,
        IJobAlertMatchRepository matchRepo,
        IJobRepository jobRepo,
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IJobAlertService jobAlertService,
        ILogger<JobAlertProcessor> logger)
    {
        _alertRepo = alertRepo;
        _matchRepo = matchRepo;
        _jobRepo = jobRepo;
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _jobAlertService = jobAlertService;
        _logger = logger;
    }

    /// <summary>
    /// Process immediate alerts for a newly created job
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessJobAlertsForJobAsync(string jobId)
    {
        try
        {
            _logger.LogInformation("Processing immediate alerts for job {JobId}", jobId);

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found", jobId);
                return;
            }

            // Get all active "Immediate" alerts
            var immediateAlerts = await _alertRepo.GetActiveAlertsByFrequencyAsync("Immediate");
            var alertsList = immediateAlerts.ToList();

            _logger.LogInformation("Found {Count} immediate alerts to check", alertsList.Count);

            foreach (var alert in alertsList)
            {
                var matches = await _jobAlertService.FindMatchingJobsAsync(alert, new[] { job });
                
                if (matches.Any())
                {
                    await ProcessMatchAsync(alert, job);
                }
            }

            _logger.LogInformation("Completed processing immediate alerts for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job alerts for job {JobId}", jobId);
            throw;
        }
    }

    /// <summary>
    /// Process daily alerts - check for new jobs in last 24 hours
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessDailyAlertsAsync()
    {
        try
        {
            _logger.LogInformation("Starting daily job alert processing");

            // Get all active "Daily" alerts
            var dailyAlerts = await _alertRepo.GetActiveAlertsByFrequencyAsync("Daily");
            var alertsList = dailyAlerts.ToList();

            _logger.LogInformation("Found {Count} daily alerts to process", alertsList.Count);

            // Get new jobs from last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var newJobs = await _jobRepo.GetJobsCreatedAfterAsync(yesterday);
            var jobsList = newJobs.Where(j => j.Status == "Open").ToList(); // Changed "Active" to "Open"

            _logger.LogInformation("Found {Count} new jobs to check", jobsList.Count);

            foreach (var alert in alertsList)
            {
                await ProcessAlertAsync(alert, jobsList);
            }

            _logger.LogInformation("Completed daily job alert processing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing daily alerts");
            throw;
        }
    }

    /// <summary>
    /// Send weekly digest to users with "Weekly" frequency
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendWeeklyDigestAsync()
    {
        try
        {
            _logger.LogInformation("Starting weekly digest processing");

            // Get all active "Weekly" alerts
            var weeklyAlerts = await _alertRepo.GetActiveAlertsByFrequencyAsync("Weekly");
            var alertsList = weeklyAlerts.ToList();

            _logger.LogInformation("Found {Count} weekly alerts to process", alertsList.Count);

            // Get new jobs from last 7 days
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            var newJobs = await _jobRepo.GetJobsCreatedAfterAsync(lastWeek);
            var jobsList = newJobs.Where(j => j.Status == "Open").ToList(); // Changed "Active" to "Open"

            _logger.LogInformation("Found {Count} jobs from last week", jobsList.Count);

            // Group alerts by user
            var alertsByUser = alertsList.GroupBy(a => a.UserId);

            foreach (var userAlerts in alertsByUser)
            {
                await SendWeeklyDigestForUserAsync(userAlerts.Key, userAlerts.ToList(), jobsList);
            }

            _logger.LogInformation("Completed weekly digest processing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending weekly digest");
            throw;
        }
    }

    private async Task ProcessAlertAsync(JobAlert alert, List<Job> jobs)
    {
        try
        {
            var matchedJobs = await _jobAlertService.FindMatchingJobsAsync(alert, jobs);
            var matchesList = matchedJobs.ToList();

            if (matchesList.Any())
            {
                _logger.LogInformation(
                    "Found {Count} matches for alert {AlertId}", 
                    matchesList.Count, 
                    alert.Id);

                foreach (var job in matchesList)
                {
                    await ProcessMatchAsync(alert, job);
                }

                // Update alert statistics
                alert.LastTriggeredAt = DateTime.UtcNow;
                alert.MatchCount += matchesList.Count;
                await _alertRepo.UpdateAsync(alert);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing alert {AlertId}", alert.Id);
        }
    }

    private async Task ProcessMatchAsync(JobAlert alert, Job job)
    {
        try
        {
            // Check if match already exists
            if (await _matchRepo.MatchExistsAsync(alert.Id, job.Id))
            {
                return;
            }

            var user = await _userRepo.GetByIdAsync(alert.UserId);
            if (user == null) return;

            var company = await _companyRepo.GetByIdAsync(job.CompanyId);
            var matchScore = await _jobAlertService.GetMatchScoreAsync(alert, job);

            // Save match record
            var match = new JobAlertMatch
            {
                JobAlertId = alert.Id,
                JobId = job.Id,
                UserId = alert.UserId,
                MatchedAt = DateTime.UtcNow,
                MatchScore = matchScore
            };

            // Send in-app notification if enabled
            if (alert.InAppEnabled)
            {
                var notification = new Notification
                {
                    UserId = alert.UserId,
                    Type = "JobMatch",
                    Title = "New Job Match Found!",
                    Message = $"A new job matching your alert '{alert.Title}' has been posted: {job.Title}",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new { jobId = job.Id, alertId = alert.Id }),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationService.CreateAsync(notification);
                match.NotificationSent = true;

                _logger.LogInformation(
                    "Sent in-app notification for job match. User: {UserId}, Job: {JobId}", 
                    alert.UserId, 
                    job.Id);
            }

            // Send email notification if enabled
            if (alert.EmailEnabled && !string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    var budgetInfo = job.BudgetAmount.HasValue && job.BudgetAmount.Value > 0 // Changed from job.Budget
                        ? $"{job.BudgetAmount.Value:N0} VND"  // Changed from job.Budget
                        : "Negotiable";

                    await _emailService.SendJobMatchEmailAsync(
                        user.Email,
                        user.FullName,
                        job.Title,
                        job.Id,
                        company?.Name ?? "Unknown Company", // Changed from CompanyName
                        job.Location ?? "Remote",
                        budgetInfo
                    );

                    match.EmailSent = true;

                    _logger.LogInformation(
                        "Sent email notification for job match. User: {Email}, Job: {JobId}", 
                        user.Email, 
                        job.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send email for job match. User: {UserId}, Job: {JobId}", 
                        alert.UserId, job.Id);
                }
            }

            await _matchRepo.AddAsync(match);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing match for alert {AlertId} and job {JobId}", alert.Id, job.Id);
        }
    }

    private async Task SendWeeklyDigestForUserAsync(string userId, List<JobAlert> alerts, List<Job> jobs)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var allMatches = new List<(Job job, int score, string alertTitle)>();

            foreach (var alert in alerts)
            {
                var matches = await _jobAlertService.FindMatchingJobsAsync(alert, jobs);
                
                foreach (var job in matches)
                {
                    // Skip if already notified
                    if (await _matchRepo.MatchExistsAsync(alert.Id, job.Id))
                        continue;

                    var score = await _jobAlertService.GetMatchScoreAsync(alert, job);
                    allMatches.Add((job, score, alert.Title ?? "Job Alert"));

                    // Record the match
                    await _matchRepo.AddAsync(new JobAlertMatch
                    {
                        JobAlertId = alert.Id,
                        JobId = job.Id,
                        UserId = userId,
                        MatchedAt = DateTime.UtcNow,
                        MatchScore = score,
                        EmailSent = true
                    });
                }
            }

            if (allMatches.Any())
            {
                // Get company info for each job
                var matchInfos = new List<Application.DTOs.JobAlerts.JobMatchInfo>();
                
                foreach (var (job, score, alertTitle) in allMatches.OrderByDescending(m => m.score).Take(10))
                {
                    var company = await _companyRepo.GetByIdAsync(job.CompanyId);
                    matchInfos.Add(new Application.DTOs.JobAlerts.JobMatchInfo(
                        job.Id,
                        job.Title,
                        company?.Name ?? "Unknown", // Changed from CompanyName
                        job.Location ?? "Remote",
                        job.BudgetAmount.HasValue && job.BudgetAmount.Value > 0 // Changed from job.Budget
                            ? $"{job.BudgetAmount.Value:N0} VND" 
                            : "Negotiable",
                        score
                    ));
                }

                await _emailService.SendJobAlertDigestEmailAsync(
                    user.Email,
                    user.FullName,
                    matchInfos,
                    "Weekly"
                );

                _logger.LogInformation(
                    "Sent weekly digest to {Email} with {Count} matches", 
                    user.Email, 
                    matchInfos.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending weekly digest for user {UserId}", userId);
        }
    }
}