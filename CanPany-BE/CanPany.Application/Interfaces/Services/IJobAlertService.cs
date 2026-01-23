using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for managing job alerts
/// </summary>
public interface IJobAlertService
{
    /// <summary>
    /// Get all active job alerts
    /// </summary>
    Task<IEnumerable<JobAlert>> GetActiveAlertsAsync();

    /// <summary>
    /// Get active job alerts for a specific user
    /// </summary>
    Task<IEnumerable<JobAlert>> GetActiveAlertsByUserIdAsync(string userId);

    /// <summary>
    /// Check if a job matches the criteria of a job alert
    /// </summary>
    /// <param name="job">The job to check</param>
    /// <param name="alert">The job alert criteria</param>
    /// <returns>True if the job matches the alert criteria</returns>
    bool CheckJobMatchesAlert(Job job, JobAlert alert);

    /// <summary>
    /// Find all job alerts that match a given job
    /// </summary>
    /// <param name="job">The job to match against alerts</param>
    /// <returns>List of matching job alerts</returns>
    Task<IEnumerable<JobAlert>> FindMatchingAlertsAsync(Job job);
}

