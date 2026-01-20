using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// User entity - Aggregate Root
/// </summary>
[BsonIgnoreExtraElements]
public class User : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = "Candidate"; // Guest/Candidate/Company/Admin

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("isLocked")]
    public bool IsLocked { get; set; } = false;

    [BsonElement("lockedUntil")]
    public DateTime? LockedUntil { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }

    // Navigation properties (not stored in MongoDB, loaded separately)
    [BsonIgnore]
    public UserProfile? Profile { get; set; }
    
    [BsonIgnore]
    public Company? Company { get; set; }
    
    [BsonIgnore]
    public Wallet? Wallet { get; set; }
}

