using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Application entity - Unified candidate response (absorbs former Proposal entity).
/// Represents a candidate's application for a job.
/// </summary>
[BsonIgnoreExtraElements]
public class Application : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = null!;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = null!;

    [BsonElement("cvId"), BsonRepresentation(BsonType.ObjectId)]
    public string? CVId { get; set; }

    [BsonElement("coverLetter")]
    public string? CoverLetter { get; set; }

    [BsonElement("proposedAmount")]
    public decimal? ProposedAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, Shortlisted, Accepted, Rejected, Withdrawn

    [BsonElement("matchScore")]
    public decimal? MatchScore { get; set; }

    [BsonElement("rejectedReason")]
    public string? RejectedReason { get; set; }

    [BsonElement("privateNotes")]
    public string? PrivateNotes { get; set; }

    [BsonElement("contractId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ContractId { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

