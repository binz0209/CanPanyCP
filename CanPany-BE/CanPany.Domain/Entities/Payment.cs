using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// Payment entity - Represents a payment transaction
/// </summary>
[BsonIgnoreExtraElements]
public class Payment : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("contractId"), BsonRepresentation(BsonType.ObjectId)]
    public string? ContractId { get; set; }

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("walletId"), BsonRepresentation(BsonType.ObjectId)]
    public string? WalletId { get; set; }

    [BsonElement("purpose")]
    public string Purpose { get; set; } = "TopUp"; // TopUp/Contract

    [BsonElement("amount")]
    public long Amount { get; set; } // VND minor units

    [BsonElement("currency")]
    public string Currency { get; set; } = "VND";

    [BsonElement("status")]
    public string Status { get; set; } = "Pending"; // Pending/Paid/Failed

    [BsonElement("paidAt")]
    public DateTime? PaidAt { get; set; }

    // VNPay fields
    [BsonElement("vnp_TxnRef")]
    public string? Vnp_TxnRef { get; set; }

    [BsonElement("vnp_TransactionNo")]
    public string? Vnp_TransactionNo { get; set; }

    [BsonElement("vnp_ResponseCode")]
    public string? Vnp_ResponseCode { get; set; }

    [BsonElement("vnp_BankCode")]
    public string? Vnp_BankCode { get; set; }

    [BsonElement("vnp_CardType")]
    public string? Vnp_CardType { get; set; }

    [BsonElement("vnp_PayDate")]
    public string? Vnp_PayDate { get; set; }

    [BsonElement("vnp_SecureHash")]
    public string? Vnp_SecureHash { get; set; }

    [BsonElement("vnp_RawQuery")]
    public string? Vnp_RawQuery { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


