using CanPany.Worker.Models;

namespace CanPany.Worker.Infrastructure.Queue;

/// <summary>
/// Job queue interface for enqueue/dequeue operations
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueue a job for processing
    /// </summary>
    Task<bool> EnqueueAsync(JobMessage job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeue next job for processing (blocking operation)
    /// </summary>
    Task<JobMessage?> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Move job to dead letter queue (failed after max retries)
    /// </summary>
    Task MoveToDeadLetterQueueAsync(JobMessage job, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requeue job for retry
    /// </summary>
    Task RequeueAsync(JobMessage job, TimeSpan? delay = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queue depth (number of pending jobs)
    /// </summary>
    Task<long> GetQueueDepthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledge job completion (remove from processing queue)
    /// </summary>
    Task AcknowledgeAsync(JobMessage job, CancellationToken cancellationToken = default);
}
