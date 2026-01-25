using CanPany.Worker.Models;
using CanPany.Worker.Infrastructure.Progress;

namespace CanPany.Worker.Handlers;

/// <summary>
/// Base interface for all job handlers
/// </summary>
public interface IJobHandler
{
    /// <summary>
    /// I18N keys this handler supports (pattern matching)
    /// Example: ["Job.SendEmail.*", "Job.Notification.Email.*"]
    /// </summary>
    string[] SupportedI18nKeys { get; }

    /// <summary>
    /// Execute the job
    /// </summary>
    /// <param name="job">Job message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Job execution result</returns>
    Task<JobResult> ExecuteAsync(JobMessage job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if handler can process this job
    /// </summary>
    bool CanHandle(string i18nKey);
}

/// <summary>
/// Base job handler with common functionality
/// </summary>
public abstract class BaseJobHandler : IJobHandler
{
    protected readonly ILogger Logger;
    protected IJobProgressTracker? ProgressTracker { get; set; }

    protected BaseJobHandler(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Set progress tracker (injected by worker)
    /// </summary>
    public void SetProgressTracker(IJobProgressTracker progressTracker)
    {
        ProgressTracker = progressTracker;
    }

    public abstract string[] SupportedI18nKeys { get; }

    public virtual bool CanHandle(string i18nKey)
    {
        foreach (var pattern in SupportedI18nKeys)
        {
            // Support wildcard matching
            if (pattern.EndsWith("*"))
            {
                var prefix = pattern[..^1]; // Remove *
                if (i18nKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (pattern.Equals(i18nKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public abstract Task<JobResult> ExecuteAsync(JobMessage job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deserialize job payload to typed object
    /// </summary>
    protected T? DeserializePayload<T>(string payload)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(payload);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[DESERIALIZE_FAILED] Payload: {Payload}", payload);
            return default;
        }
    }

    /// <summary>
    /// Report job progress (0-100%)
    /// </summary>
    protected async Task ReportProgressAsync(
        string jobId,
        int percentComplete,
        string? currentStep = null,
        Dictionary<string, object>? details = null,
        CancellationToken cancellationToken = default)
    {
        if (ProgressTracker != null)
        {
            await ProgressTracker.UpdateProgressAsync(jobId, percentComplete, currentStep, details, cancellationToken);
        }
    }

    /// <summary>
    /// Report job progress by steps
    /// </summary>
    protected async Task ReportStepsAsync(
        string jobId,
        int completedSteps,
        int totalSteps,
        string? currentStep = null,
        CancellationToken cancellationToken = default)
    {
        if (ProgressTracker != null)
        {
            await ProgressTracker.UpdateStepsAsync(jobId, completedSteps, totalSteps, currentStep, cancellationToken);
        }
    }
}
