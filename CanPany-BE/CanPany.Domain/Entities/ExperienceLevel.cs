using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// ExperienceLevel entity - Standardized experience/seniority levels managed by admin.
/// Used across UserProfile, Job, and search filters.
/// </summary>
[BsonIgnoreExtraElements]
public class ExperienceLevel : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Sort order for display (Intern=1, Fresher=2, Junior=3, etc.)
    /// </summary>
    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
