using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Application entity - Job application from Candidate
/// </summary>
public class Application : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = null!;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = null!;

    [BsonElement("cvId"), BsonRepresentation(BsonType.ObjectId)]
    public string? CVId { get; set; } // CV used for this application

    [BsonElement("coverLetter")]
    public string? CoverLetter { get; set; }

    [BsonElement("expectedSalary")]
    public decimal? ExpectedSalary { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Withdrawn

    [BsonElement("matchScore")]
    public decimal? MatchScore { get; set; } // AI-calculated match score

    [BsonElement("rejectedReason")]
    public string? RejectedReason { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

