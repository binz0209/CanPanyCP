using CanPany.Shared.Common.Base;
using CanPany.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// FilterPreset entity - Represents saved filter presets for reuse in searches
/// </summary>
[BsonIgnoreExtraElements]
public class FilterPreset : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string? Id { get; set; }

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty; // Preset name

    [BsonElement("filterType")]
    [BsonRepresentation(BsonType.String)]
    public FilterType FilterType { get; set; } // JobSearch, CandidateSearch

    [BsonElement("filters")]
    public Dictionary<string, object> Filters { get; set; } = new(); // JSON serialized filters

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

