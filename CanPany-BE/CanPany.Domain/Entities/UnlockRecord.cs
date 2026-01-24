using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// UnlockRecord entity - Tracks which company has unlocked which candidate's contact info
/// </summary>
[BsonIgnoreExtraElements]
public class UnlockRecord : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("companyId"), BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = string.Empty;

    [BsonElement("feeAmount")]
    public decimal FeeAmount { get; set; }

    [BsonElement("unlockedAt")]
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
