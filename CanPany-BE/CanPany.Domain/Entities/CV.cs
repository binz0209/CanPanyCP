using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// CV entity - Candidate CV file storage.
/// Analysis data is stored in CVAnalysis (single source of truth).
/// </summary>
[BsonIgnoreExtraElements]
public class CV : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("fileUrl")]
    public string FileUrl { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [BsonElement("cloudinaryPublicId")]
    public string? CloudinaryPublicId { get; set; }

    [BsonElement("isDefault")]
    public bool IsDefault { get; set; } = false;

    [BsonElement("latestAnalysisId"), BsonRepresentation(BsonType.ObjectId)]
    public string? LatestAnalysisId { get; set; }

    [BsonElement("extractedSkills")]
    public List<string> ExtractedSkills { get; set; } = new();

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

