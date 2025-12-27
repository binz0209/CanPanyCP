using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// ProjectSkill entity - Represents the many-to-many relationship between Projects and Skills
/// </summary>
[BsonIgnoreExtraElements]
public class ProjectSkill : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("projectId"), BsonRepresentation(BsonType.ObjectId)]
    public string ProjectId { get; set; } = string.Empty;

    [BsonElement("skillId"), BsonRepresentation(BsonType.ObjectId)]
    public string SkillId { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


