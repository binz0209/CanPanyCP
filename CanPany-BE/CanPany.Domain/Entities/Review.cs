using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Review entity - Represents a review/rating after contract completion.
/// Linked to Contract (not Project).
/// </summary>
[BsonIgnoreExtraElements]
public class Review : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("contractId"), BsonRepresentation(BsonType.ObjectId)]
    public string ContractId { get; set; } = string.Empty;

    [BsonElement("reviewerId"), BsonRepresentation(BsonType.ObjectId)]
    public string ReviewerId { get; set; } = string.Empty;

    [BsonElement("revieweeId"), BsonRepresentation(BsonType.ObjectId)]
    public string RevieweeId { get; set; } = string.Empty;

    [BsonElement("rating")]
    public int Rating { get; set; } // 1-5

    [BsonElement("comment")]
    public string? Comment { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


