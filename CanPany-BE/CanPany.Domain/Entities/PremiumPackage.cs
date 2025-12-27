using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// PremiumPackage entity - Represents premium subscription packages
/// </summary>
[BsonIgnoreExtraElements]
public class PremiumPackage : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("userType")]
    public string UserType { get; set; } = "Candidate"; // Candidate/Company

    [BsonElement("packageType")]
    public string PackageType { get; set; } = string.Empty; // AIPremium/JobPosting/AIScreening

    [BsonElement("price")]
    public long Price { get; set; } // VND minor units

    [BsonElement("durationDays")]
    public int DurationDays { get; set; } = 30;

    [BsonElement("features")]
    public List<string> Features { get; set; } = new();

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}


