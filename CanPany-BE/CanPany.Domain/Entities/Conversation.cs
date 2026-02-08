using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Conversation entity - First-class conversation between two users.
/// Replaces the former computed conversationKey approach.
/// </summary>
[BsonIgnoreExtraElements]
public class Conversation : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("participantIds")]
    public List<string> ParticipantIds { get; set; } = new(); // Exactly 2 user IDs (sorted)

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string? JobId { get; set; }

    [BsonElement("lastMessageAt")]
    public DateTime? LastMessageAt { get; set; }

    [BsonElement("lastMessagePreview")]
    public string? LastMessagePreview { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}
