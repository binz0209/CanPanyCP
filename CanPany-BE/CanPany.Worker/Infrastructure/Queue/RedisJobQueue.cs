using CanPany.Worker.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CanPany.Worker.Infrastructure.Queue;

/// <summary>
/// Redis-based job queue implementation
/// Uses Redis Lists for FIFO queue operations
/// </summary>
public class RedisJobQueue : IJobQueue, IAsyncDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisJobQueue> _logger;
    private readonly string _jobQueueKey;
    private readonly string _processingQueueKey;
    private readonly string _deadLetterQueueKey;

    public RedisJobQueue(
        IConnectionMultiplexer redis,
        ILogger<RedisJobQueue> logger,
        IConfiguration configuration)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;

        _jobQueueKey = configuration["Redis:JobQueueKey"] ?? "canpany:jobs:queue";
        _processingQueueKey = configuration["Redis:ProcessingQueueKey"] ?? "canpany:jobs:processing";
        _deadLetterQueueKey = configuration["Redis:DeadLetterQueueKey"] ?? "canpany:jobs:dlq";
    }

    public async Task<bool> EnqueueAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(job);
            
            // Use Sorted Set with negative priority as score
            // Higher priority (Critical=3) gets lower score (-3) so it's processed first
            var score = -(int)job.Priority;
            var added = await _db.SortedSetAddAsync(_jobQueueKey, json, score);

            _logger.LogInformation(
                "[REDIS_ENQUEUE] JobId: {JobId} | I18nKey: {I18nKey} | Priority: {Priority} | Added: {Added}",
                job.JobId,
                job.I18nKey,
                job.Priority,
                added
            );

            return added;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[REDIS_ENQUEUE_FAILED] JobId: {JobId}", job.JobId);
            return false;
        }
    }

    public async Task<JobMessage?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // ZPOPMIN: Pop job with lowest score (highest priority)
            // Score format: -priority (so -3 (Critical) comes before -1 (Normal))
            var results = await _db.SortedSetPopAsync(_jobQueueKey, Order.Ascending);

            if (!results.HasValue || results.Value.Element.IsNullOrEmpty)
                return null;

            var json = results.Value.Element.ToString();
            var job = JsonSerializer.Deserialize<JobMessage>(json);

            if (job != null)
            {
                _logger.LogDebug(
                    "[REDIS_DEQUEUE] JobId: {JobId} | I18nKey: {I18nKey}",
                    job.JobId,
                    job.I18nKey
                );
            }

            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[REDIS_DEQUEUE_FAILED]");
            return null;
        }
    }

    public async Task MoveToDeadLetterQueueAsync(JobMessage job, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            // Add failure metadata
            job.Metadata ??= new Dictionary<string, string>();
            job.Metadata["FailureReason"] = reason;
            job.Metadata["FailedAt"] = DateTime.UtcNow.ToString("O");

            var json = JsonSerializer.Serialize(job);
            
            await _db.ListRightPushAsync(_deadLetterQueueKey, json);

            // Remove from processing queue
            await _db.ListRemoveAsync(_processingQueueKey, json);

            _logger.LogWarning(
                "[REDIS_MOVE_TO_DLQ] JobId: {JobId} | I18nKey: {I18nKey} | Reason: {Reason}",
                job.JobId,
                job.I18nKey,
                reason
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[REDIS_DLQ_FAILED] JobId: {JobId}", job.JobId);
        }
    }

    public async Task RequeueAsync(JobMessage job, TimeSpan? delay = null, CancellationToken cancellationToken = default)
    {
        try
        {
            job.RetryCount++;

            // Remove from processing queue
            var json = JsonSerializer.Serialize(job);
            await _db.ListRemoveAsync(_processingQueueKey, json);

            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                // For delayed retry, use Redis Sorted Set with score = Unix timestamp
                var executeAt = DateTimeOffset.UtcNow.Add(delay.Value).ToUnixTimeSeconds();
                await _db.SortedSetAddAsync($"{_jobQueueKey}:delayed", json, executeAt);

                _logger.LogInformation(
                    "[REDIS_REQUEUE_DELAYED] JobId: {JobId} | RetryCount: {RetryCount} | Delay: {Delay}s",
                    job.JobId,
                    job.RetryCount,
                    delay.Value.TotalSeconds
                );
            }
            else
            {
                // Immediate retry - back to main queue
                await _db.ListRightPushAsync(_jobQueueKey, json);

                _logger.LogInformation(
                    "[REDIS_REQUEUE] JobId: {JobId} | RetryCount: {RetryCount}",
                    job.JobId,
                    job.RetryCount
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[REDIS_REQUEUE_FAILED] JobId: {JobId}", job.JobId);
        }
    }

    public async Task<long> GetQueueDepthAsync(CancellationToken cancellationToken = default)
    {
        return await _db.ListLengthAsync(_jobQueueKey);
    }

    public async Task AcknowledgeAsync(JobMessage job, CancellationToken cancellationToken = default)
    {
        // Job already removed from Sorted Set queue when dequeued (ZPOPMIN)
        // No need for separate ACK since we're using Sorted Set, not List
        _logger.LogDebug(
            "[REDIS_ACK] JobId: {JobId} | Already removed from queue during dequeue",
            job.JobId
        );
        
        await Task.CompletedTask; // Keep async signature for interface compatibility
    }

    public async ValueTask DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
        }
    }
}
