using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Company entity - Aggregate Root
/// </summary>
public class Company : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("logoUrl")]
    public string? LogoUrl { get; set; }

    [BsonElement("website")]
    public string? Website { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("verificationStatus")]
    public string VerificationStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

    [BsonElement("verificationDocuments")]
    public List<string> VerificationDocuments { get; set; } = new(); // URLs to documents

    [BsonElement("verifiedAt")]
    public DateTime? VerifiedAt { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

