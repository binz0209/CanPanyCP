using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Service for tracking user-job interactions (implicit feedback for CF).
/// Converts interaction types to implicit scores and avoids duplicates.
/// </summary>
public class InteractionTrackingService : IInteractionTrackingService
{
    private readonly IUserJobInteractionRepository _interactionRepo;
    private readonly ILogger<InteractionTrackingService> _logger;

    public InteractionTrackingService(
        IUserJobInteractionRepository interactionRepo,
        ILogger<InteractionTrackingService> logger)
    {
        _interactionRepo = interactionRepo;
        _logger = logger;
    }

    public async Task<UserJobInteraction?> TrackInteractionAsync(string userId, string jobId, InteractionType type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            // Check for duplicate interaction of same type
            var existing = await _interactionRepo.GetByUserJobAndTypeAsync(userId, jobId, type);
            if (existing != null)
            {
                _logger.LogDebug("Interaction already exists: {UserId} - {JobId} - {Type}", userId, jobId, type);
                return existing;
            }

            var interaction = new UserJobInteraction
            {
                UserId = userId,
                JobId = jobId,
                Type = type,
                Score = GetImplicitScore(type),
                CreatedAt = DateTime.UtcNow
            };

            var saved = await _interactionRepo.AddAsync(interaction);
            _logger.LogInformation("Tracked interaction: {UserId} - {JobId} - {Type} (score: {Score})",
                userId, jobId, type, interaction.Score);

            return saved;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error tracking interaction: {UserId} - {JobId} - {Type}", userId, jobId, type);
            throw;
        }
    }

    public async Task<long> GetUserInteractionCountAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return 0;

        return await _interactionRepo.GetCountByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserJobInteraction>> GetUserInteractionsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Enumerable.Empty<UserJobInteraction>();

        return await _interactionRepo.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Convert interaction type to implicit score.
    /// Higher scores indicate stronger interest signals.
    /// </summary>
    public static double GetImplicitScore(InteractionType type) => type switch
    {
        InteractionType.View => 1.0,
        InteractionType.Click => 2.0,
        InteractionType.Bookmark => 3.0,
        InteractionType.Apply => 5.0,
        _ => 1.0
    };
}
