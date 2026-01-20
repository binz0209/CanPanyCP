using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Notification entity - Represents a system notification for a user
/// </summary>
[BsonIgnoreExtraElements]
public class Notification : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // ProposalAccepted, NewMessage, etc.

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("payload")]
    public string? Payload { get; set; } // JSON payload

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


