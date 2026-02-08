using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Message entity - Represents a message within a conversation.
/// </summary>
[BsonIgnoreExtraElements]
public class Message : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("conversationId"), BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = string.Empty;

    [BsonElement("senderId"), BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


