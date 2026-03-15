using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Hybrid Recommendation service combining Semantic Search and Collaborative Filtering
/// </summary>
public interface IHybridRecommendationService
{
    /// <summary>
    /// Get recommended jobs for a candidate using hybrid scoring (semantic + CF).
    /// Uses adaptive alpha for cold start handling.
    /// </summary>
    Task<IEnumerable<(Job Job, double HybridScore)>> GetRecommendedJobsAsync(string userId, int limit = 10);

    /// <summary>
    /// Calculate hybrid scores for a list of jobs for a specific user.
    /// Used for sorting search results by relevance.
    /// </summary>
    Task<Dictionary<string, double>> CalculateScoresForJobsAsync(string userId, IEnumerable<Job> jobs);
}
