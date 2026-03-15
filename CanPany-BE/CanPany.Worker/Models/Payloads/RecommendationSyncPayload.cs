namespace CanPany.Worker.Models.Payloads;

/// <summary>
/// Payload for syncing recommendation skills/profile context
/// before generating job recommendations.
/// </summary>
public record RecommendationSyncPayload
{
    /// <summary>
    /// User ID to sync recommendation data for.
    /// </summary>
    public string UserId { get; init; } = null!;

    /// <summary>
    /// Number of recommendations to warm up.
    /// </summary>
    public int Limit { get; init; } = 20;
}
