using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Contract entity - Represents a formal contract created from an accepted proposal
/// </summary>
[BsonIgnoreExtraElements]
public class Contract : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("projectId"), BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("clientId"), BsonRepresentation(BsonType.ObjectId)]
    public string ClientId { get; set; } = string.Empty;

    [BsonElement("freelancerId"), BsonRepresentation(BsonType.ObjectId)]
    public string FreelancerId { get; set; } = string.Empty;

    [BsonElement("agreedAmount")]
    public decimal AgreedAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // Active/Completed/Cancelled

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}


