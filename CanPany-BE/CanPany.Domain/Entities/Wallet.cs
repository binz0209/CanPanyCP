using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Wallet entity - User's digital wallet. Aggregate Root.
/// </summary>
[BsonIgnoreExtraElements]
public class Wallet : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("balance")]
    public long Balance { get; set; } = 0;

    [BsonElement("currency")]
    public string Currency { get; set; } = "VND";

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

