using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// JobAlert entity - Represents job alerts for candidates
/// </summary>
[BsonIgnoreExtraElements]
public class JobAlert : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty; // CandidateId

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty; // Alert name

    [BsonElement("skillIds")]
    public List<string>? SkillIds { get; set; } // Skills filter

    [BsonElement("categoryId"), BsonRepresentation(BsonType.ObjectId)]
    public string? CategoryId { get; set; } // Category filter

    [BsonElement("location")]
    public string? Location { get; set; } // Location filter

    [BsonElement("minBudget")]
    public decimal? MinBudget { get; set; } // Min budget filter

    [BsonElement("maxBudget")]
    public decimal? MaxBudget { get; set; } // Max budget filter

    [BsonElement("isRemote")]
    public bool? IsRemote { get; set; } // Remote filter

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

