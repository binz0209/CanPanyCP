namespace CanPany.Application.DTOs.Payments;

/// <summary>
/// Request to create a SePay checkout session
/// </summary>
public class SePayCheckoutRequest
{
    public string OrderInvoiceNumber { get; set; } = string.Empty;
    public long OrderAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public string OrderDescription { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "BANK_TRANSFER"; // BANK_TRANSFER | NAPAS_BANK_TRANSFER
    public string? CustomerId { get; set; }
    public string? SuccessUrl { get; set; }
    public string? ErrorUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? CustomData { get; set; }
}

/// <summary>
/// Result of creating SePay checkout - fields to include in the payment form
/// </summary>
public class SePayCheckoutResult
{
    /// <summary>
    /// The form action URL to POST to (SePay checkout page)
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>
    /// All hidden form fields (including the HMAC-SHA256 signature)
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = new();

    /// <summary>
    /// Internal payment ID in our system (MongoDB ObjectId)
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;
}
