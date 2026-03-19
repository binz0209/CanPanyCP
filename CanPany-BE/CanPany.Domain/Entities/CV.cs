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

    /// <summary>
    /// Structured CV data (JSON) for AI-generated, editable CVs.
    /// When set, FileUrl is empty — PDF is generated client-side on download.
    /// </summary>
    [BsonElement("structuredData")]
    public CVStructuredData? StructuredData { get; set; }

    [BsonElement("isAIGenerated")]
    public bool IsAIGenerated { get; set; } = false;
}

/// <summary>Structured CV data returned by Gemini and editable by the user.</summary>
public class CVStructuredData
{
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string LinkedIn { get; set; } = string.Empty;
    public string GitHub { get; set; } = string.Empty;
    public string Portfolio { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<CVExperienceEntry> Experience { get; set; } = new();
    public List<CVEducationEntry> Education { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public string? TargetJobTitle { get; set; }
}

public class CVExperienceEntry
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public List<string> Bullets { get; set; } = new();
}

public class CVEducationEntry
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

