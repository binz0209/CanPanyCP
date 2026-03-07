using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Job Alert Match entity - Tracks which jobs matched which alerts (prevent duplicates)
/// </summary>
[BsonIgnoreExtraElements]
public class JobAlertMatch
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("jobAlertId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobAlertId { get; set; } = string.Empty;

    [BsonElement("jobId"), BsonRepresentation(BsonType.ObjectId)]
    public string JobId { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("matchedAt")]
    public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("notificationSent")]
    public bool NotificationSent { get; set; } = false;

    [BsonElement("emailSent")]
    public bool EmailSent { get; set; } = false;

    [BsonElement("matchScore")]
    public int MatchScore { get; set; } = 0; // 0-100
}