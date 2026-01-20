using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// UserSettings entity - Represents user preferences and settings
/// </summary>
[BsonIgnoreExtraElements]
public class UserSettings : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("notificationSettings")]
    public NotificationSettings NotificationSettings { get; set; } = new();

    [BsonElement("privacySettings")]
    public PrivacySettings PrivacySettings { get; set; } = new();

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

public class NotificationSettings
{
    [BsonElement("emailNotifications")]
    public bool EmailNotifications { get; set; } = true;

    [BsonElement("messageNotifications")]
    public bool MessageNotifications { get; set; } = true;

    [BsonElement("newProjectNotifications")]
    public bool NewProjectNotifications { get; set; } = true;
}

public class PrivacySettings
{
    [BsonElement("publicProfile")]
    public bool PublicProfile { get; set; } = true;

    [BsonElement("showOnlineStatus")]
    public bool ShowOnlineStatus { get; set; } = false;
}


