using CanPany.Application.DTOs.Payments;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Payment service interface
/// </summary>
public interface IPaymentService
{
    Task<Payment?> GetByIdAsync(string id);
    Task<IEnumerable<Payment>> GetByUserIdAsync(string userId);
    Task<Payment> CreateDepositRequestAsync(string userId, long amount);
    Task<Payment> CreatePremiumPurchaseAsync(string userId, string packageId, long amount);

    /// <summary>
    /// Creates a pending Payment record and generates SePay checkout form fields.
    /// Frontend uses the returned CheckoutUrl + Fields to render a POST form.
    /// </summary>
    Task<SePayCheckoutResult> CreateSePayCheckoutAsync(
        string userId,
        long amount,
        string purpose,
        string? packageId,
        string successUrl,
        string errorUrl,
        string cancelUrl);

    /// <summary>
    /// Processes a SePay webhook callback after the user completes payment.
    /// Verifies signature, updates payment status, and credits wallet on success.
    /// </summary>
    Task<bool> ProcessSePayCallbackAsync(Dictionary<string, string> webhookData);

    Task<bool> ProcessPaymentAsync(string paymentId, Dictionary<string, string> paymentData);
    Task<bool> ApprovePaymentAsync(string paymentId);
    Task<bool> RejectPaymentAsync(string paymentId, string reason);
}


