using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Project entity - Represents a freelance project posted by a company
/// </summary>
[BsonIgnoreExtraElements]
public class Project : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("ownerId"), BsonRepresentation(BsonType.ObjectId)]
    public string OwnerId { get; set; } = string.Empty;

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
    public string BudgetType { get; set; } = "Fixed"; // Fixed/Hourly

    [BsonElement("budgetAmount")]
    public decimal? BudgetAmount { get; set; }

    [BsonElement("deadline")]
    public DateTime? Deadline { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Open"; // Open/InProgress/Completed/Cancelled

    [BsonElement("images")]
    public List<string> Images { get; set; } = new();

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}


