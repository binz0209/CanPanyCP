namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Collaborative Filtering service for computing CF-based recommendation scores
/// </summary>
public interface ICollaborativeFilteringService
{
    /// <summary>
    /// Get CF predicted score for a specific user-job pair using user-based kNN
    /// </summary>
    Task<double> GetCfScoreAsync(string userId, string jobId);

    /// <summary>
    /// Get CF predicted scores for multiple jobs at once (batch scoring)
    /// </summary>
    Task<Dictionary<string, double>> GetCfScoresForJobsAsync(string userId, IEnumerable<string> jobIds);

    /// <summary>
    /// Train and save a CF model snapshot to the database (UC-49).
    /// Called by Hangfire recurring job.
    /// </summary>
    Task TrainAndSaveModelAsync();
}
