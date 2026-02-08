using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// UserSubscription entity - Tracks premium subscription lifecycle.
/// Created when a premium payment succeeds.
/// </summary>
[BsonIgnoreExtraElements]
public class UserSubscription : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("packageId"), BsonRepresentation(BsonType.ObjectId)]
    public string PackageId { get; set; } = string.Empty;

    [BsonElement("paymentId"), BsonRepresentation(BsonType.ObjectId)]
    public string PaymentId { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Active"; // Active, Expired, Cancelled

    [BsonElement("startDate")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    [BsonElement("features")]
    public List<string> Features { get; set; } = new();

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}
