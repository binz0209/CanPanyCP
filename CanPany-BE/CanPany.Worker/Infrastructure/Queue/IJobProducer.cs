using CanPany.Worker.Models;

namespace CanPany.Worker.Infrastructure.Queue;

/// <summary>
/// Job producer interface for enqueueing jobs
/// </summary>
public interface IJobProducer
{
    /// <summary>
    /// Enqueue a job to be processed by workers
    /// </summary>
    Task<string> EnqueueJobAsync(JobMessage job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enqueue a job with specific type and payload
    /// </summary>
    Task<string> EnqueueJobAsync(string jobType, object payload, int priority = 0, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Schedule a job for future execution
    /// </summary>
    Task<string> ScheduleJobAsync(JobMessage job, TimeSpan delay, CancellationToken cancellationToken = default);
}
