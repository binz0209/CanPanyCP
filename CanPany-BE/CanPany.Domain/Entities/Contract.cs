using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Contract entity - Represents a formal contract created from an accepted application.
/// Linked to Job (not Project).
/// </summary>
[BsonIgnoreExtraElements]
public class Contract : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = string.Empty;

    [BsonElement("applicationId"), BsonRepresentation(BsonType.ObjectId)]
    public string ApplicationId { get; set; } = string.Empty;

    [BsonElement("companyId"), BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = string.Empty;

    [BsonElement("agreedAmount")]
    public decimal AgreedAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // Active, InProgress, Completed, Cancelled, Disputed, Resolved

    [BsonElement("startDate")]
    public DateTime? StartDate { get; set; }

    [BsonElement("endDate")]
    public DateTime? EndDate { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [BsonElement("cancellationReason")]
    public string? CancellationReason { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}


