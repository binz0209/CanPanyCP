namespace CanPany.Worker.Models;

/// <summary>
/// Job progress information
/// </summary>
public class JobProgress
{
    /// <summary>
    /// Job ID
    /// </summary>
    public string JobId { get; set; } = null!;

    /// <summary>
    /// Job status
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int PercentComplete { get; set; } = 0;

    /// <summary>
    /// Current step description
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Total steps
    /// </summary>
    public int TotalSteps { get; set; } = 0;

    /// <summary>
    /// Completed steps
    /// </summary>
    public int CompletedSteps { get; set; } = 0;

    /// <summary>
    /// Additional progress details
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Job result data
    /// </summary>
    public Dictionary<string, object?>? Result { get; set; }

    /// <summary>
    /// When the job started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the job completed (success or failure)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Job status
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job is pending/queued
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Job is currently running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Job completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Job failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Job was cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Job is retrying after failure
    /// </summary>
    Retrying = 5
}
