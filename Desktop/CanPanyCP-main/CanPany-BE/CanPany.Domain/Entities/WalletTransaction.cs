using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// WalletTransaction entity - Represents a transaction in a user's wallet
/// </summary>
[BsonIgnoreExtraElements]
public class WalletTransaction : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("walletId"), BsonRepresentation(BsonType.ObjectId)]
    public string WalletId { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("paymentId"), BsonRepresentation(BsonType.ObjectId)]
    public string? PaymentId { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = "TopUp"; // TopUp/Withdraw/Hold/Release

    [BsonElement("amount")]
    public long Amount { get; set; }

    [BsonElement("balanceAfter")]
    public long BalanceAfter { get; set; }

    [BsonElement("note")]
    public string? Note { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


