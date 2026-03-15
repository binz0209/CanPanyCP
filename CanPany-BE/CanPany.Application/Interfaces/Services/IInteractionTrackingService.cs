using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service for tracking user-job interactions (implicit feedback for CF)
/// </summary>
public interface IInteractionTrackingService
{
    /// <summary>
    /// Track an interaction between a user and a job.
    /// Avoids duplicates for same (userId, jobId, type) combination.
    /// </summary>
    Task<UserJobInteraction?> TrackInteractionAsync(string userId, string jobId, InteractionType type);

    /// <summary>
    /// Get the total number of interactions for a user (for cold start detection)
    /// </summary>
    Task<long> GetUserInteractionCountAsync(string userId);

    /// <summary>
    /// Get all interactions for a user
    /// </summary>
    Task<IEnumerable<UserJobInteraction>> GetUserInteractionsAsync(string userId);
}
