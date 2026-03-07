using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Job Alert entity - Represents a user's job alert with filters
/// </summary>
[BsonIgnoreExtraElements]
public class JobAlert : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("title")]
    public string? Title { get; set; }

    // Search criteria
    [BsonElement("skillIds")]
    public List<string>? SkillIds { get; set; }

    [BsonElement("categoryIds")]
    public List<string>? CategoryIds { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("jobType")]
    public string? JobType { get; set; } // FullTime, PartTime, Freelance

    [BsonElement("minBudget")]
    public decimal? MinBudget { get; set; }

    [BsonElement("maxBudget")]
    public decimal? MaxBudget { get; set; }

    [BsonElement("experienceLevel")]
    public string? ExperienceLevel { get; set; }

    // Alert settings
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("frequency")]
    public string Frequency { get; set; } = "Daily"; // Immediate, Daily, Weekly

    [BsonElement("emailEnabled")]
    public bool EmailEnabled { get; set; } = true;

    [BsonElement("inAppEnabled")]
    public bool InAppEnabled { get; set; } = true;

    // Tracking
    [BsonElement("lastTriggeredAt")]
    public DateTime? LastTriggeredAt { get; set; }

    [BsonElement("matchCount")]
    public int MatchCount { get; set; } = 0;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

