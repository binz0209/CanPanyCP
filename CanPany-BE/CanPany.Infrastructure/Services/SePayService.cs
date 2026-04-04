using CanPany.Application.DTOs.Payments;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// SePay Payment Gateway service implementation.
/// Handles HMAC-SHA256 signature generation and webhook verification.
/// Algorithm: sort keys alphabetically → join as "key=value&..." → HMAC-SHA256 with secret_key
/// </summary>
public class SePayService : ISePayService
{
    private readonly SePayOptions _options;
    private readonly ILogger<SePayService> _logger;

    // SePay checkout endpoint templates
    private const string SandboxCheckoutUrl = "https://pay-sandbox.sepay.vn/{0}/checkout/init";
    private const string ProductionCheckoutUrl = "https://pay.sepay.vn/{0}/checkout/init";

    public SePayService(IOptions<SePayOptions> options, ILogger<SePayService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GenerateCheckoutUrl()
    {
        var template = _options.Env == "production"
            ? ProductionCheckoutUrl
            : SandboxCheckoutUrl;

        return string.Format(template, _options.CheckoutVersion);
    }

    /// <inheritdoc/>
    private static readonly string[] AllowedFieldsOrder = new[] {
        "merchant", "env", "operation", "payment_method", "order_amount",
        "currency", "order_invoice_number", "order_description", "customer_id",
        "agreement_id", "agreement_name", "agreement_type", "agreement_payment_frequency",
        "agreement_amount_per_payment", "success_url", "error_url", "cancel_url", "order_id"
    };

    public Dictionary<string, string> GenerateCheckoutFields(SePayCheckoutRequest request)
    {
        // Build all available fields without order
        var rawFields = new Dictionary<string, string>
        {
            ["merchant"] = _options.MerchantId,
            ["operation"] = "PURCHASE",
            ["payment_method"] = request.PaymentMethod,
            ["order_invoice_number"] = request.OrderInvoiceNumber,
            ["order_amount"] = request.OrderAmount.ToString(),
            ["currency"] = request.Currency,
            ["order_description"] = request.OrderDescription
        };

        if (!string.IsNullOrEmpty(_options.Env) && _options.Env != "production")
            rawFields["env"] = _options.Env;

        if (!string.IsNullOrEmpty(request.CustomerId))
            rawFields["customer_id"] = request.CustomerId;

        if (!string.IsNullOrEmpty(request.SuccessUrl))
            rawFields["success_url"] = request.SuccessUrl;

        if (!string.IsNullOrEmpty(request.ErrorUrl))
            rawFields["error_url"] = request.ErrorUrl;

        if (!string.IsNullOrEmpty(request.CancelUrl))
            rawFields["cancel_url"] = request.CancelUrl;

        if (!string.IsNullOrEmpty(request.CustomData))
            rawFields["custom_data"] = request.CustomData;

        // Generate HMAC-SHA256 signature using strict ordering
        rawFields["signature"] = ComputeSignature(rawFields);

        // Finally, return the dictionary that will be sent to the front-end.
        // We order it by the official SePay array order so the HTML form POST order is guaranteed safe!
        var orderedResult = new Dictionary<string, string>();
        foreach (var key in AllowedFieldsOrder)
        {
            if (rawFields.TryGetValue(key, out var val))
            {
                orderedResult[key] = val;
            }
        }
        
        // Add any remaining keys that weren't in the allowed signature array (like custom_data, signature)
        foreach (var kvp in rawFields)
        {
            if (!orderedResult.ContainsKey(kvp.Key))
            {
                orderedResult[kvp.Key] = kvp.Value;
            }
        }

        _logger.LogDebug("Generated SePay checkout fields for order {OrderInvoiceNumber}", request.OrderInvoiceNumber);

        return orderedResult;
    }

    /// <summary>
    /// Computes HMAC-SHA256 signature over fields based on SePay PG SDK logic.
    /// Replicates sepay-pg-node:
    /// - Join with comma ','
    /// - Output as Base64 string
    /// </summary>
    private string ComputeSignature(Dictionary<string, string> fields)
    {
        var signed = new List<string>();
        foreach (var key in AllowedFieldsOrder)
        {
            if (fields.TryGetValue(key, out var val))
            {
                signed.Add($"{key}={val}");
            }
        }

        var dataToSign = string.Join(",", signed);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
        return Convert.ToBase64String(hash);
    }

    /// <inheritdoc/>
    public bool VerifySecretKey(string providedKey)
    {
        return string.Equals(_options.SecretKey, providedKey, StringComparison.Ordinal);
    }
}
