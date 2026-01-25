using CanPany.Worker.Models;

namespace CanPany.Worker.Infrastructure.Progress;

/// <summary>
/// Interface for job progress tracking
/// </summary>
public interface IJobProgressTracker
{
    /// <summary>
    /// Initialize job progress
    /// </summary>
    Task InitializeAsync(string jobId, int totalSteps = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update job progress
    /// </summary>
    Task UpdateProgressAsync(
        string jobId,
        int percentComplete,
        string? currentStep = null,
        Dictionary<string, object>? details = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update completed steps
    /// </summary>
    Task UpdateStepsAsync(
        string jobId,
        int completedSteps,
        int totalSteps,
        string? currentStep = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark job as running
    /// </summary>
    Task MarkAsRunningAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark job as completed
    /// </summary>
    Task MarkAsCompletedAsync(
        string jobId,
        Dictionary<string, object?>? result = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark job as failed
    /// </summary>
    Task MarkAsFailedAsync(
        string jobId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark job as retrying
    /// </summary>
    Task MarkAsRetryingAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get job progress
    /// </summary>
    Task<JobProgress?> GetProgressAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete job progress (cleanup)
    /// </summary>
    Task DeleteProgressAsync(string jobId, CancellationToken cancellationToken = default);
}
