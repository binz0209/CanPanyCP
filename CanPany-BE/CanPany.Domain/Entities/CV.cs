using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// CV entity - Aggregate Root
/// </summary>
public class CV : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("fileUrl")]
    public string FileUrl { get; set; } = string.Empty; // Cloudinary URL

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [BsonElement("isDefault")]
    public bool IsDefault { get; set; } = false;

    [BsonElement("extractedSkills")]
    public List<string> ExtractedSkills { get; set; } = new(); // Skills extracted by AI

    [BsonElement("extractedContent")]
    public string? ExtractedContent { get; set; } // Full text extracted from CV

    [BsonElement("atsScore")]
    public decimal? AtsScore { get; set; } // ATS compatibility score

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

