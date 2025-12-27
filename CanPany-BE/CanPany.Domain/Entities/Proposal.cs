using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Proposal entity - Represents a candidate's application for a project
/// </summary>
[BsonIgnoreExtraElements]
public class Proposal : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("projectId"), BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("freelancerId"), BsonRepresentation(BsonType.ObjectId)]
    public string FreelancerId { get; set; } = string.Empty;

    [BsonElement("coverLetter")]
    public string CoverLetter { get; set; } = string.Empty;

    [BsonElement("bidAmount")]
    public decimal? BidAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending/Accepted/Rejected

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}


