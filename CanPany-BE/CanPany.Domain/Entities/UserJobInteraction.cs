using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Interaction types for user-job interactions (implicit feedback).
/// Higher values indicate stronger interest signals.
/// </summary>
public enum InteractionType
{
    View = 1,
    Click = 2,
    Bookmark = 3,
    Apply = 4
}

/// <summary>
/// UserJobInteraction entity - Tracks implicit user-job interactions for Collaborative Filtering.
/// Each record represents a single interaction event between a candidate and a job.
/// </summary>
[BsonIgnoreExtraElements]
public class UserJobInteraction : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = string.Empty;

    [BsonElement("type")]
    public InteractionType Type { get; set; }

    /// <summary>
    /// Implicit score derived from interaction type:
    /// View=1, Click=2, Bookmark=3, Apply=5
    /// </summary>
    [BsonElement("score")]
    public double Score { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
