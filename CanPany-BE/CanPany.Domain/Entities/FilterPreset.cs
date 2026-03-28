using CanPany.Domain.Enums;
using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// FilterPreset entity - Stores saved filter configurations for job/candidate search
/// </summary>
[BsonIgnoreExtraElements]
public class FilterPreset : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("filterType")]
    [BsonRepresentation(BsonType.String)]
    public FilterPresetType FilterType { get; set; }

    [BsonElement("filters")]
    public string Filters { get; set; } = "{}"; // JSON string

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}
