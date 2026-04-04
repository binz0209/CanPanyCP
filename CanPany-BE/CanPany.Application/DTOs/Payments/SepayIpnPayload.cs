namespace CanPany.Application.DTOs.Payments;

using System.Text.Json.Serialization;

/// <summary>
/// Model hứng payload JSON từ SePay gửi về qua IPN (Instant Payment Notification).
/// Tham khảo: https://docs.sepay.vn
/// </summary>
public class SepayIpnPayload
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("notification_type")]
    public string? NotificationType { get; set; }

    [JsonPropertyName("order")]
    public SepayIpnOrder? Order { get; set; }

    [JsonPropertyName("transaction")]
    public SepayIpnTransaction? Transaction { get; set; }

    [JsonPropertyName("customer")]
    public SepayIpnCustomer? Customer { get; set; }

    [JsonPropertyName("agreement")]
    public SepayIpnAgreement? Agreement { get; set; }
}

public class SepayIpnOrder
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }

    [JsonPropertyName("order_status")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("order_currency")]
    public string? OrderCurrency { get; set; }

    [JsonPropertyName("order_amount")]
    public string? OrderAmount { get; set; }

    [JsonPropertyName("order_invoice_number")]
    public string? OrderInvoiceNumber { get; set; }

    [JsonPropertyName("order_description")]
    public string? OrderDescription { get; set; }

    [JsonPropertyName("custom_data")]
    public object? CustomData { get; set; }

    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }
}

public class SepayIpnTransaction
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("transaction_type")]
    public string? TransactionType { get; set; }

    [JsonPropertyName("transaction_date")]
    public string? TransactionDate { get; set; }

    [JsonPropertyName("transaction_status")]
    public string? TransactionStatus { get; set; }

    [JsonPropertyName("transaction_amount")]
    public string? TransactionAmount { get; set; }

    [JsonPropertyName("transaction_currency")]
    public string? TransactionCurrency { get; set; }

    [JsonPropertyName("authentication_status")]
    public string? AuthenticationStatus { get; set; }

    [JsonPropertyName("card_number")]
    public string? CardNumber { get; set; }

    [JsonPropertyName("card_holder_name")]
    public string? CardHolderName { get; set; }

    [JsonPropertyName("card_expiry")]
    public string? CardExpiry { get; set; }

    [JsonPropertyName("card_funding_method")]
    public string? CardFundingMethod { get; set; }

    [JsonPropertyName("card_brand")]
    public string? CardBrand { get; set; }
}

public class SepayIpnCustomer
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; set; }
}

public class SepayIpnAgreement
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("agreement_id")]
    public string? AgreementId { get; set; }
}

/// <summary>
/// Response trả về cho SePay sau khi nhận IPN.
/// SePay yêu cầu luôn trả về HTTP 200, kèm code để biết trạng thái xử lý.
/// </summary>
public class SepayIpnResponse
{
    /// <summary>"00" = đã xử lý thành công; mã khác = có lỗi nhưng đã nhận được</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "00";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "OK";
}
