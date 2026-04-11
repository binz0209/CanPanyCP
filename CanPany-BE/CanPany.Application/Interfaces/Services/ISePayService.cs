using CanPany.Application.DTOs.Payments;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// SePay Payment Gateway service - handles signature generation and checkout URL building
/// </summary>
public interface ISePayService
{
    /// <summary>
    /// Returns the checkout endpoint URL based on configured environment (sandbox/production)
    /// </summary>
    string GenerateCheckoutUrl();

    /// <summary>
    /// Builds the form field dictionary with HMAC-SHA256 signature for SePay checkout
    /// </summary>
    Dictionary<string, string> GenerateCheckoutFields(SePayCheckoutRequest request);

    /// <summary>
    /// Verifies if a provided secret key matches the configured SePay SecretKey
    /// </summary>
    bool VerifySecretKey(string providedKey);
}
