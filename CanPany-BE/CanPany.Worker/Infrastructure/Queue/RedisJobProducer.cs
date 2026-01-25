using CanPany.Worker.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CanPany.Worker.Infrastructure.Queue;

/// <summary>
/// Redis-based job producer
/// </summary>
public class RedisJobProducer : IJobProducer
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisJobProducer> _logger;
    private readonly string _queueKey;

    public RedisJobProducer(
        IConnectionMultiplexer redis,
        ILogger<RedisJobProducer> logger,
        IConfiguration configuration)
    {
        _redis = redis;
        _logger = logger;
        _queueKey = configuration["Redis:JobQueueKey"] ?? "canpany:jobs:queue";
    }

    public async Task<string> EnqueueJobAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(job);
            
            // Use sorted set with priority (lower number = higher priority)
            var score = -(int)job.Priority; // Negative to get highest priority first
            await db.SortedSetAddAsync(_queueKey, json, score);
            
            _logger.LogInformation(
                "[JOB_ENQUEUED] JobId: {JobId} | I18nKey: {I18nKey} | Priority: {Priority}",
                job.JobId,
                job.I18nKey,
                job.Priority
            );
            
            return job.JobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JOB_ENQUEUE_ERROR] Failed to enqueue job: {I18nKey}", job.I18nKey);
            throw;
        }
    }

    public async Task<string> EnqueueJobAsync(
        string jobType,
        object payload,
        int priority = 0,
        CancellationToken cancellationToken = default)
    {
        var job = new JobMessage
        {
            JobId = Guid.NewGuid().ToString(),
            I18nKey = jobType,
            Payload = JsonSerializer.Serialize(payload),
            Priority = (JobPriority)priority,
            EnqueuedAt = DateTime.UtcNow
        };

        return await EnqueueJobAsync(job, cancellationToken);
    }

    public async Task<string> ScheduleJobAsync(
        JobMessage job,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            
            // Set scheduled time
            job.ScheduledAt = DateTime.UtcNow.Add(delay);
            
            var json = JsonSerializer.Serialize(job);
            
            // Use sorted set with timestamp as score for scheduled jobs
            var scheduleKey = $"{_queueKey}:scheduled";
            var executeAt = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
            
            await db.SortedSetAddAsync(scheduleKey, json, executeAt);
            
            _logger.LogInformation(
                "[JOB_SCHEDULED] JobId: {JobId} | I18nKey: {I18nKey} | ExecuteAt: {ExecuteAt}",
                job.JobId,
                job.I18nKey,
                DateTimeOffset.FromUnixTimeSeconds(executeAt)
            );
            
            return job.JobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JOB_SCHEDULE_ERROR] Failed to schedule job: {I18nKey}", job.I18nKey);
            throw;
        }
    }
}
