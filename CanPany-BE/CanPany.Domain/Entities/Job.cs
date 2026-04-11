using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Job entity - Unified work opportunity (absorbs former Project entity).
/// Aggregate Root.
/// </summary>
[BsonIgnoreExtraElements]
public class Job : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("companyId"), BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = null!;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("categoryId"), BsonRepresentation(BsonType.ObjectId)]
    public string? CategoryId { get; set; }

    [BsonElement("skillIds")]
    public List<string> SkillIds { get; set; } = new();

    [BsonElement("skillEmbedding")]
    public List<double>? SkillEmbedding { get; set; }

    [BsonElement("budgetType")]
    public string BudgetType { get; set; } = "Fixed"; // Fixed, Hourly

    [BsonElement("budgetAmount")]
    public decimal? BudgetAmount { get; set; }

    [BsonElement("level")]
    public string? Level { get; set; } // Junior, Mid, Senior, Expert

    [BsonElement("experienceLevelId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ExperienceLevelId { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("locationId"), BsonRepresentation(BsonType.ObjectId)]
    public string? LocationId { get; set; }

    [BsonElement("isRemote")]
    public bool IsRemote { get; set; } = false;

    [BsonElement("deadline")]
    public DateTime? Deadline { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Open"; // Draft, Open, Closed, Cancelled, Hidden

    [BsonElement("engagementType")]
    public string EngagementType { get; set; } = "DirectHire"; // DirectHire, FreelanceProject

    [BsonElement("images")]
    public List<string> Images { get; set; } = new();

    [BsonElement("viewCount")]
    public int ViewCount { get; set; } = 0;

    [BsonElement("applicationCount")]
    public int ApplicationCount { get; set; } = 0;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

