using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// CandidateAlert entity - Represents candidate alerts for companies
/// </summary>
[BsonIgnoreExtraElements]
public class CandidateAlert : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("companyId"), BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty; // Alert name

    [BsonElement("skillIds")]
    public List<string>? SkillIds { get; set; } // Skills filter

    [BsonElement("location")]
    public string? Location { get; set; } // Location filter

    [BsonElement("minExperience")]
    public int? MinExperience { get; set; } // Min years of experience

    [BsonElement("maxExperience")]
    public int? MaxExperience { get; set; } // Max years of experience

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

