using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Message entity - Represents a message between users
/// </summary>
[BsonIgnoreExtraElements]
public class Message : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("conversationKey")]
    public string ConversationKey { get; set; } = string.Empty;

    [BsonElement("projectId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ProjectId { get; set; }

    [BsonElement("senderId"), BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; } = string.Empty;

    [BsonElement("receiverId"), BsonRepresentation(BsonType.ObjectId)]
    public string ReceiverId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


