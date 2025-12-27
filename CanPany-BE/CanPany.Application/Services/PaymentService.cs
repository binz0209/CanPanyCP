using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IWalletService _walletService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository repo,
        IWalletService walletService,
        ILogger<PaymentService> logger)
    {
        _repo = repo;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<Payment?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Payment ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by ID: {PaymentId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<Payment> CreateDepositRequestAsync(string userId, long amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            var payment = new Payment
            {
                UserId = userId,
                Purpose = "TopUp",
                Amount = amount,
                Currency = "VND",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _repo.AddAsync(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deposit request: {UserId}, {Amount}", userId, amount);
            throw;
        }
    }

    public async Task<Payment> CreatePremiumPurchaseAsync(string userId, string packageId, long amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(packageId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            var payment = new Payment
            {
                UserId = userId,
                Purpose = "Premium",
                Amount = amount,
                Currency = "VND",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            return await _repo.AddAsync(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premium purchase: {UserId}, {PackageId}", userId, packageId);
            throw;
        }
    }

    public async Task<bool> ProcessPaymentAsync(string paymentId, Dictionary<string, string> paymentData)
    {
        try
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            // Update payment with VNPay/SePay data
            payment.Vnp_TxnRef = paymentData.GetValueOrDefault("vnp_TxnRef");
            payment.Vnp_TransactionNo = paymentData.GetValueOrDefault("vnp_TransactionNo");
            payment.Vnp_ResponseCode = paymentData.GetValueOrDefault("vnp_ResponseCode");
            payment.Vnp_BankCode = paymentData.GetValueOrDefault("vnp_BankCode");
            payment.Vnp_PayDate = paymentData.GetValueOrDefault("vnp_PayDate");

            if (payment.Vnp_ResponseCode == "00") // Success
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;

                // Update wallet if TopUp
                if (payment.Purpose == "TopUp")
                {
                    var (succeeded, errors, _) = await _walletService.ChangeBalanceAsync(payment.UserId, payment.Amount, "Deposit");
                    if (!succeeded)
                    {
                        _logger.LogError("Failed to update wallet for payment: {PaymentId}", paymentId);
                    }
                }
            }
            else
            {
                payment.Status = "Failed";
            }

            payment.MarkAsUpdated();
            await _repo.UpdateAsync(payment);
            return payment.Status == "Paid";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<bool> ApprovePaymentAsync(string paymentId)
    {
        try
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = "Paid";
            payment.PaidAt = DateTime.UtcNow;
            payment.MarkAsUpdated();
            await _repo.UpdateAsync(payment);

            // Update wallet if TopUp
            if (payment.Purpose == "TopUp")
            {
                var (succeeded, errors, _) = await _walletService.ChangeBalanceAsync(payment.UserId, payment.Amount, "Deposit approved");
                if (!succeeded)
                {
                    _logger.LogError("Failed to update wallet for approved payment: {PaymentId}", paymentId);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payment: {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<bool> RejectPaymentAsync(string paymentId, string reason)
    {
        try
        {
            var payment = await _repo.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = "Failed";
            payment.MarkAsUpdated();
            await _repo.UpdateAsync(payment);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment: {PaymentId}", paymentId);
            throw;
        }
    }
}


