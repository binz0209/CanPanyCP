using CanPany.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CanPany.Worker.Infrastructure.Progress;

/// <summary>
/// Redis-based job progress tracker
/// </summary>
public class RedisJobProgressTracker : IJobProgressTracker
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisJobProgressTracker> _logger;
    private readonly string _progressKeyPrefix;
    private readonly TimeSpan _progressExpiry;

    public RedisJobProgressTracker(
        IConnectionMultiplexer redis,
        ILogger<RedisJobProgressTracker> logger,
        IConfiguration configuration)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;

        _progressKeyPrefix = configuration["Redis:ProgressKeyPrefix"] ?? "canpany:job:progress:";
        // Keep progress for 24 hours after job completion
        _progressExpiry = TimeSpan.FromHours(configuration.GetValue<int>("Redis:ProgressExpiryHours", 24));
    }

    private string GetProgressKey(string jobId) => $"{_progressKeyPrefix}{jobId}";
    private string GetUserJobsKey(string userId) => $"canpany:user:jobs:{userId}";
    private static readonly TimeSpan UserJobsExpiry = TimeSpan.FromDays(30);

    public async Task InitializeAsync(
        string jobId,
        int totalSteps = 0,
        string? userId = null,
        string? jobType = null,
        string? jobTitle = null,
        CancellationToken cancellationToken = default)
    {
        var progress = new JobProgress
        {
            JobId = jobId,
            UserId = userId,
            JobType = jobType,
            JobTitle = jobTitle,
            Status = JobStatus.Pending,
            PercentComplete = 0,
            TotalSteps = totalSteps,
            CompletedSteps = 0,
            UpdatedAt = DateTime.UtcNow
        };

        await SaveProgressAsync(progress, cancellationToken);

        // Index this jobId under the user's job list (sorted by timestamp score)
        if (!string.IsNullOrEmpty(userId))
        {
            var userKey = GetUserJobsKey(userId);
            var score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _db.SortedSetAddAsync(userKey, jobId, score);
            await _db.KeyExpireAsync(userKey, UserJobsExpiry);
        }

        _logger.LogDebug("[PROGRESS_INIT] JobId: {JobId} | UserId: {UserId} | JobType: {JobType} | TotalSteps: {TotalSteps}",
            jobId, userId, jobType, totalSteps);
    }

    public async Task UpdateProgressAsync(
        string jobId,
        int percentComplete,
        string? currentStep = null,
        Dictionary<string, object>? details = null,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.PercentComplete = Math.Clamp(percentComplete, 0, 100);
        progress.CurrentStep = currentStep;
        if (details != null)
        {
            progress.Details = details;
        }
        progress.UpdatedAt = DateTime.UtcNow;

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogDebug(
            "[PROGRESS_UPDATE] JobId: {JobId} | Progress: {Percent}% | Step: {Step}",
            jobId,
            percentComplete,
            currentStep
        );
    }

    public async Task UpdateStepsAsync(
        string jobId,
        int completedSteps,
        int totalSteps,
        string? currentStep = null,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.CompletedSteps = completedSteps;
        progress.TotalSteps = totalSteps;
        progress.CurrentStep = currentStep;
        progress.UpdatedAt = DateTime.UtcNow;

        // Calculate percentage
        if (totalSteps > 0)
        {
            progress.PercentComplete = (int)Math.Round((double)completedSteps / totalSteps * 100);
        }

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogDebug(
            "[PROGRESS_STEPS] JobId: {JobId} | Steps: {Completed}/{Total} ({Percent}%)",
            jobId,
            completedSteps,
            totalSteps,
            progress.PercentComplete
        );
    }

    public async Task MarkAsRunningAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.Status = JobStatus.Running;
        progress.StartedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogInformation("[JOB_STARTED] JobId: {JobId}", jobId);
    }

    public async Task MarkAsCompletedAsync(
        string jobId,
        Dictionary<string, object?>? result = null,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.Status = JobStatus.Completed;
        progress.PercentComplete = 100;
        progress.CompletedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;
        progress.Result = result;

        if (progress.StartedAt.HasValue)
        {
            progress.DurationMs = (long)(progress.CompletedAt.Value - progress.StartedAt.Value).TotalMilliseconds;
        }

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogInformation(
            "[JOB_COMPLETED] JobId: {JobId} | Duration: {Duration}ms",
            jobId,
            progress.DurationMs
        );
    }

    public async Task MarkAsFailedAsync(
        string jobId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.Status = JobStatus.Failed;
        progress.ErrorMessage = errorMessage;
        progress.CompletedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;

        if (progress.StartedAt.HasValue)
        {
            progress.DurationMs = (long)(progress.CompletedAt.Value - progress.StartedAt.Value).TotalMilliseconds;
        }

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogWarning(
            "[JOB_FAILED] JobId: {JobId} | Error: {Error}",
            jobId,
            errorMessage
        );
    }

    public async Task MarkAsRetryingAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var progress = await GetProgressAsync(jobId, cancellationToken) ?? new JobProgress { JobId = jobId };

        progress.Status = JobStatus.Retrying;
        progress.UpdatedAt = DateTime.UtcNow;

        await SaveProgressAsync(progress, cancellationToken);

        _logger.LogInformation("[JOB_RETRYING] JobId: {JobId}", jobId);
    }

    public async Task<JobProgress?> GetProgressAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetProgressKey(jobId);
            var json = await _db.StringGetAsync(key);

            if (json.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<JobProgress>(json!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PROGRESS_GET_FAILED] JobId: {JobId}", jobId);
            return null;
        }
    }

    public async Task DeleteProgressAsync(string jobId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetProgressKey(jobId);
            await _db.KeyDeleteAsync(key);

            _logger.LogDebug("[PROGRESS_DELETED] JobId: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PROGRESS_DELETE_FAILED] JobId: {JobId}", jobId);
        }
    }

    public async Task<List<JobProgress>> GetUserJobsAsync(
        string userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userKey = GetUserJobsKey(userId);
            // Sorted set, score = enqueue timestamp ms → newest first via Desc order
            var jobIds = await _db.SortedSetRangeByRankAsync(
                userKey,
                start: skip,
                stop: skip + take - 1,
                order: StackExchange.Redis.Order.Descending);

            var results = new List<JobProgress>();
            foreach (var jobId in jobIds)
            {
                var progress = await GetProgressAsync(jobId!, cancellationToken);
                if (progress != null)
                    results.Add(progress);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[USER_JOBS_GET_FAILED] UserId: {UserId}", userId);
            return new List<JobProgress>();
        }
    }


    private async Task SaveProgressAsync(JobProgress progress, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetProgressKey(progress.JobId);
            var json = JsonSerializer.Serialize(progress);

            await _db.StringSetAsync(key, json, _progressExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PROGRESS_SAVE_FAILED] JobId: {JobId}", progress.JobId);
        }
    }
}
