namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for triggering background job matching
/// </summary>
public interface IJobMatchingService
{
    /// <summary>
    /// Trigger job alert matching for a newly created job
    /// </summary>
    /// <param name="jobId">The ID of the job to match</param>
    void TriggerJobAlertMatching(string jobId);
}

