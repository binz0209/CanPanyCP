namespace CanPany.Domain.Entities;

/// <summary>
/// SePay Payment Gateway configuration options
/// </summary>
public class SePayOptions
{
    /// <summary>
    /// Merchant ID from my.sepay.vn
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// Secret key from my.sepay.vn
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Environment: "sandbox" or "production"
    /// </summary>
    public string Env { get; set; } = "sandbox";

    /// <summary>
    /// API version (default: "v1")
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// Checkout page version (default: "v1")
    /// </summary>
    public string CheckoutVersion { get; set; } = "v1";
}
