using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// Service for triggering job matching background jobs
/// </summary>
public class JobMatchingService : IJobMatchingService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobMatchingService> _logger;

    public JobMatchingService(
        IBackgroundJobClient backgroundJobClient,
        IJobRepository jobRepository,
        ILogger<JobMatchingService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public void TriggerJobAlertMatching(string jobId)
    {
        try
        {
            // Queue the job matching as a background job
            var hangfireJobId = _backgroundJobClient.Enqueue<Jobs.JobMatchProcessor>(
                processor => processor.ProcessJobAlertsForJobAsync(jobId));

            _logger.LogInformation(
                "Queued job alert matching for job {JobId} with Hangfire job {HangfireJobId}",
                jobId,
                hangfireJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue job alert matching for job {JobId}", jobId);
            // Don't throw - job creation should succeed even if matching fails
        }
    }
}

