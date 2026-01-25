using CanPany.Worker.Infrastructure.Queue;
using CanPany.Worker.Infrastructure.Progress;
using CanPany.Worker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CanPany.Api.Controllers;

/// <summary>
/// Background Jobs API - Test và quản lý background jobs (worker tasks)
/// </summary>
[ApiController]
[Route("api/background-jobs")]
public class BackgroundJobsController : ControllerBase
{
    private readonly IJobProducer _jobProducer;
    private readonly IJobProgressTracker _progressTracker;
    private readonly ILogger<BackgroundJobsController> _logger;

    public BackgroundJobsController(
        IJobProducer jobProducer,
        IJobProgressTracker progressTracker,
        ILogger<BackgroundJobsController> logger)
    {
        _jobProducer = jobProducer;
        _progressTracker = progressTracker;
        _logger = logger;
    }

    /// <summary>
    /// 📧 Test Send Email Job
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/background-jobs/test/send-email
    ///     {
    ///         "to": "user@example.com",
    ///         "subject": "Test Email",
    ///         "body": "This is a test email from background worker",
    ///         "isHtml": false
    ///     }
    /// 
    /// </remarks>
    [HttpPost("test/send-email")]
    public async Task<IActionResult> TestSendEmail([FromBody] SendEmailRequest request)
    {
        var payload = new
        {
            To = request.To,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml
        };

        var jobId = await _jobProducer.EnqueueJobAsync(
            jobType: "Job.SendEmail.Test",  // Match handler pattern: Job.SendEmail.*
            payload: payload,
            priority: (int)JobPriority.Normal
        );

        _logger.LogInformation("[API] Enqueued email job: {JobId}", jobId);

        return Accepted(new
        {
            jobId,
            status = "queued",
            message = "Email job has been queued for processing",
            estimatedTime = "1-5 seconds",
            checkWorkerLogs = "Check Worker console for processing logs"
        });
    }

    /// <summary>
    /// 🤖 Test AI Matching Job
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/background-jobs/test/ai-matching
    ///     {
    ///         "userId": "user-123",
    ///         "preferences": {
    ///             "skills": ["C#", "React"],
    ///             "experience": "3-5 years"
    ///         },
    ///         "maxResults": 10
    ///     }
    /// 
    /// </remarks>
    [HttpPost("test/ai-matching")]
    public async Task<IActionResult> TestAIMatching([FromBody] AIMatchingRequest request)
    {
        var payload = new
        {
            UserId = request.UserId,
            Preferences = request.Preferences,
            MaxResults = request.MaxResults
        };

        var jobId = await _jobProducer.EnqueueJobAsync(
            jobType: "Job.AIMatching.Test",  // Match handler pattern: Job.AIMatching.*
            payload: payload,
            priority: (int)JobPriority.High
        );

        _logger.LogInformation("[API] Enqueued AI matching job: {JobId}", jobId);

        return Accepted(new
        {
            jobId,
            status = "queued",
            message = "AI matching job has been queued for processing",
            estimatedTime = "5-30 seconds",
            checkWorkerLogs = "Check Worker console for processing logs"
        });
    }

    /// <summary>
    /// 📊 Test Generate Report Job
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/background-jobs/test/generate-report
    ///     {
    ///         "reportType": "monthly-summary",
    ///         "startDate": "2024-01-01",
    ///         "endDate": "2024-01-31",
    ///         "filters": {
    ///             "department": "Engineering"
    ///         }
    ///     }
    /// 
    /// </remarks>
    [HttpPost("test/generate-report")]
    public async Task<IActionResult> TestGenerateReport([FromBody] GenerateReportRequest request)
    {
        var payload = new
        {
            ReportType = request.ReportType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Filters = request.Filters
        };

        var jobId = await _jobProducer.EnqueueJobAsync(
            jobType: "Job.GenerateReport.Test",  // Match handler pattern: Job.GenerateReport.*
            payload: payload,
            priority: (int)JobPriority.Low
        );

        _logger.LogInformation("[API] Enqueued report generation job: {JobId}", jobId);

        return Accepted(new
        {
            jobId,
            status = "queued",
            message = "Report generation job has been queued for processing",
            estimatedTime = "10-60 seconds",
            checkWorkerLogs = "Check Worker console for processing logs"
        });
    }

    /// <summary>
    /// 📅 Schedule a job for later execution
    /// </summary>
    /// <remarks>
    /// Example request:
    /// 
    ///     POST /api/background-jobs/schedule
    ///     {
    ///         "jobType": "Jobs.Email.Send",
    ///         "payload": {
    ///             "to": "user@example.com",
    ///             "subject": "Scheduled Email"
    ///         },
    ///         "priority": 1,
    ///         "delayMinutes": 5
    ///     }
    /// 
    /// </remarks>
    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleJob([FromBody] ScheduleJobRequest request)
    {
        var job = new JobMessage
        {
            JobId = Guid.NewGuid().ToString(),
            I18nKey = request.JobType,
            Payload = JsonSerializer.Serialize(request.Payload),
            Priority = (JobPriority)request.Priority,
            EnqueuedAt = DateTime.UtcNow
        };

        var jobId = await _jobProducer.ScheduleJobAsync(
            job: job,
            delay: TimeSpan.FromMinutes(request.DelayMinutes)
        );

        var executeAt = DateTime.UtcNow.AddMinutes(request.DelayMinutes);

        _logger.LogInformation("[API] Scheduled job: {JobId} for {ExecuteAt}", jobId, executeAt);

        return Accepted(new
        {
            jobId,
            status = "scheduled",
            executeAt,
            message = $"Job scheduled to run at {executeAt:yyyy-MM-dd HH:mm:ss}"
        });
    }

    /// <summary>
    /// 🔥 Test Bulk Jobs - Enqueue multiple jobs at once (stress test)
    /// </summary>
    [HttpPost("test/bulk")]
    public async Task<IActionResult> TestBulkJobs([FromQuery] int count = 10)
    {
        if (count > 100)
        {
            return BadRequest(new { error = "Maximum 100 jobs allowed per request" });
        }

        var jobIds = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var payload = new
            {
                Index = i + 1,
                Message = $"Bulk job {i + 1} of {count}",
                Timestamp = DateTime.UtcNow
            };

            // Vary priority based on index
            int priorityInt = (i % 3) == 0 ? 2 : (i % 3) == 1 ? 1 : 0;

            var jobId = await _jobProducer.EnqueueJobAsync(
                jobType: "Job.SendEmail.Bulk",  // Match handler pattern
                payload: payload,
                priority: priorityInt
            );

            jobIds.Add(jobId);
        }

        _logger.LogInformation("[API] Enqueued {Count} bulk jobs", count);

        return Accepted(new
        {
            totalJobs = count,
            jobIds = jobIds.Take(10).ToList(), // Only show first 10
            status = "queued",
            message = $"{count} jobs have been queued for processing",
            checkWorkerLogs = "Monitor Worker console to see concurrent processing"
        });
    }

    /// <summary>
    /// 📋 Get queue info and Redis keys
    /// </summary>
    [HttpGet("queue/info")]
    public IActionResult GetQueueInfo()
    {
        return Ok(new
        {
            message = "Background job queue information",
            redis = new
            {
                mainQueue = "canpany:jobs:queue",
                scheduledQueue = "canpany:jobs:queue:scheduled",
                processingQueue = "canpany:jobs:processing",
                deadLetterQueue = "canpany:jobs:dlq"
            },
            commands = new
            {
                viewQueue = "ZRANGE canpany:jobs:queue 0 -1 WITHSCORES",
                viewScheduled = "ZRANGE canpany:jobs:queue:scheduled 0 -1 WITHSCORES",
                viewProcessing = "LRANGE canpany:jobs:processing 0 -1",
                viewDLQ = "LRANGE canpany:jobs:dlq 0 -1",
                countJobs = "ZCARD canpany:jobs:queue"
            },
            workerConfig = new
            {
                maxConcurrentJobs = 10,
                pollingInterval = "5 seconds",
                maxRetries = 3,
                note = "See appsettings.json in Worker project"
            }
        });
    }

    /// <summary>
    /// 🎯 Quick test - Send one of each job type
    /// </summary>
    [HttpPost("test/all")]
    public async Task<IActionResult> TestAllJobTypes()
    {
        var results = new List<object>();

        // 1. Email Job
        var emailJobId = await _jobProducer.EnqueueJobAsync(
            "Job.SendEmail.Bulk",
            new { To = "test@example.com", Subject = "Test", Body = "Test email" },
            (int)JobPriority.Normal
        );
        results.Add(new { type = "Email", jobId = emailJobId });

        // 2. AI Matching Job
        var aiJobId = await _jobProducer.EnqueueJobAsync(
            "Job.AIMatching.QuickTest",
            new { UserId = "user-123", Preferences = new { Skills = new[] { "C#" } } },
            (int)JobPriority.High
        );
        results.Add(new { type = "AI Matching", jobId = aiJobId });

        // 3. Report Generation Job
        var reportJobId = await _jobProducer.EnqueueJobAsync(
            "Job.GenerateReport.Monthly",
            new { ReportType = "monthly", StartDate = DateTime.UtcNow.AddMonths(-1) },
            (int)JobPriority.Low
        );
        results.Add(new { type = "Report", jobId = reportJobId });

        _logger.LogInformation("[API] Enqueued test jobs for all types");

        return Accepted(new
        {
            message = "Enqueued one job of each type",
            jobs = results,
            status = "queued",
            tip = "Check Worker console logs to see them processing"
        });
    }

    /// <summary>
    /// 📊 Get job progress by ID
    /// </summary>
    /// <remarks>
    /// Returns the current progress of a background job including:
    /// - Status (Pending, Running, Completed, Failed, Retrying)
    /// - Percent complete (0-100%)
    /// - Current step description
    /// - Result data (if completed)
    /// - Error message (if failed)
    /// 
    /// Example: GET /api/background-jobs/progress/{jobId}
    /// </remarks>
    [HttpGet("progress/{jobId}")]
    public async Task<IActionResult> GetJobProgress(string jobId)
    {
        var progress = await _progressTracker.GetProgressAsync(jobId);

        if (progress == null)
        {
            return NotFound(new
            {
                error = "Job not found",
                jobId,
                message = "Job may not have started yet or has expired (24h retention)"
            });
        }

        return Ok(progress);
    }

    /// <summary>
    /// 🔍 Get multiple job progresses
    /// </summary>
    /// <remarks>
    /// Query multiple job progresses in one request
    /// 
    /// Example: POST /api/background-jobs/progress/batch
    /// Body: ["job-id-1", "job-id-2", "job-id-3"]
    /// </remarks>
    [HttpPost("progress/batch")]
    public async Task<IActionResult> GetBatchJobProgress([FromBody] List<string> jobIds)
    {
        if (jobIds.Count > 50)
        {
            return BadRequest(new { error = "Maximum 50 job IDs allowed per request" });
        }

        var results = new Dictionary<string, JobProgress?>();

        foreach (var jobId in jobIds)
        {
            var progress = await _progressTracker.GetProgressAsync(jobId);
            results[jobId] = progress;
        }

        return Ok(new
        {
            totalRequested = jobIds.Count,
            found = results.Count(x => x.Value != null),
            notFound = results.Count(x => x.Value == null),
            results
        });
    }
}

#region Request Models

public record SendEmailRequest
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsHtml { get; init; } = false;
}

public record AIMatchingRequest
{
    public string UserId { get; init; } = string.Empty;
    public Dictionary<string, object> Preferences { get; init; } = new();
    public int MaxResults { get; init; } = 10;
}

public record GenerateReportRequest
{
    public string ReportType { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
}

public record ScheduleJobRequest
{
    public string JobType { get; init; } = string.Empty;
    public object Payload { get; init; } = new();
    public int Priority { get; init; } = 1;
    public int DelayMinutes { get; init; } = 5;
}

#endregion
