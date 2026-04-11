using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// UserConsent entity — Tracks user consent for data processing.
/// Required by Vietnamese data protection law (Nghị định 13/2023/NĐ-CP).
/// </summary>
[BsonIgnoreExtraElements]
public class UserConsent : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of consent:
    /// "DataProcessing" — General data processing consent
    /// "CrossBorderTransfer" — Sending data to external APIs (Gemini, etc.)
    /// "AIAnalysis" — AI analysis of CV/profile data
    /// "ExternalSync_GitHub" — GitHub data sync consent
    /// "ExternalSync_LinkedIn" — LinkedIn data sync consent
    /// "Marketing" — Marketing communications
    /// </summary>
    [BsonElement("consentType")]
    public string ConsentType { get; set; } = string.Empty;

    [BsonElement("isGranted")]
    public bool IsGranted { get; set; } = false;

    [BsonElement("grantedAt")]
    public DateTime? GrantedAt { get; set; }

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }

    /// <summary>Version of the privacy policy the user agreed to</summary>
    [BsonElement("policyVersion")]
    public string? PolicyVersion { get; set; }

    /// <summary>IP address at the time of consent</summary>
    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}
