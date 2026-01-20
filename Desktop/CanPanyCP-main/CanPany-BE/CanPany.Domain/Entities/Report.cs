using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Report entity - Represents user reports/complaints
/// </summary>
[BsonIgnoreExtraElements]
public class Report : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("reporterId"), BsonRepresentation(BsonType.ObjectId)]
    public string ReporterId { get; set; } = string.Empty; // UserId of the person reporting

    [BsonElement("reportedUserId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ReportedUserId { get; set; } // UserId of the person being reported

    [BsonElement("reportedCompanyId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ReportedCompanyId { get; set; } // CompanyId if reporting a company

    [BsonElement("reason")]
    public string Reason { get; set; } = string.Empty; // Reason for reporting

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty; // Detailed description

    [BsonElement("evidence")]
    public List<string>? Evidence { get; set; } // URLs of evidence: screenshots, messages, etc.

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, Resolved, Rejected

    [BsonElement("resolvedBy"), BsonRepresentation(BsonType.ObjectId)]
    public string? ResolvedBy { get; set; } // AdminId

    [BsonElement("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [BsonElement("resolutionNote")]
    public string? ResolutionNote { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

