namespace CanPany.Worker.Models;

/// <summary>
/// Job message container
/// </summary>
public class JobMessage
{
    /// <summary>
    /// Unique job identifier
    /// </summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// I18N key to identify job type and handler
    /// </summary>
    public string I18nKey { get; set; } = null!;

    /// <summary>
    /// Job payload (serialized JSON)
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User ID who triggered the job
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Job priority
    /// </summary>
    public JobPriority Priority { get; set; } = JobPriority.Normal;

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retries allowed
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// When the job was enqueued
    /// </summary>
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the job should be executed (for delayed jobs)
    /// </summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Job priority levels
/// </summary>
public enum JobPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Job execution result
/// </summary>
public record JobResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public Dictionary<string, object?>? Metadata { get; init; }

    public static JobResult SuccessResult(Dictionary<string, object?>? metadata = null) 
        => new() { Success = true, Metadata = metadata };

    public static JobResult FailureResult(string errorMessage, string? errorCode = null, Dictionary<string, object?>? metadata = null)
        => new() { Success = false, ErrorMessage = errorMessage, ErrorCode = errorCode, Metadata = metadata };
}
