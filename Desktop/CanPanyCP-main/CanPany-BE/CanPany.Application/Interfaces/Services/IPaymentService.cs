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
    Task<bool> ProcessPaymentAsync(string paymentId, Dictionary<string, string> paymentData);
    Task<bool> ApprovePaymentAsync(string paymentId);
    Task<bool> RejectPaymentAsync(string paymentId, string reason);
}


