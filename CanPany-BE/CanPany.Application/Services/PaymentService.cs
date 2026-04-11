using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.DTOs.Payments;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundEmailService _backgroundEmailService;
    private readonly IUserService _userService;
    private readonly ISePayService _sePayService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository repo,
        IWalletService walletService,
        INotificationService notificationService,
        IBackgroundEmailService backgroundEmailService,
        IUserService userService,
        ISePayService sePayService,
        ILogger<PaymentService> logger)
    {
        _repo = repo;
        _walletService = walletService;
        _notificationService = notificationService;
        _backgroundEmailService = backgroundEmailService;
        _userService = userService;
        _sePayService = sePayService;
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

    public async Task<SePayCheckoutResult> CreateSePayCheckoutAsync(
        string userId,
        long amount,
        string purpose,
        string? packageId,
        string successUrl,
        string errorUrl,
        string cancelUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            // Create a pending payment record first to get a stable ID
            var payment = new Payment
            {
                UserId = userId,
                Purpose = purpose,
                PackageId = packageId,
                Amount = amount,
                Currency = "VND",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            payment = await _repo.AddAsync(payment);

            // Use the payment ID as order invoice number for traceability
            var orderInvoiceNumber = payment.Id;

            // Build SePay checkout request
            var checkoutRequest = new SePayCheckoutRequest
            {
                OrderInvoiceNumber = orderInvoiceNumber,
                OrderAmount = amount,
                Currency = "VND",
                OrderDescription = $"Thanh toan {purpose} - {orderInvoiceNumber}",
                PaymentMethod = "BANK_TRANSFER",
                CustomerId = userId,
                SuccessUrl = successUrl,
                ErrorUrl = errorUrl,
                CancelUrl = cancelUrl,
                CustomData = payment.Id
            };

            // Store the invoice number on the payment
            payment.Sepay_OrderInvoiceNumber = orderInvoiceNumber;
            payment.MarkAsUpdated();
            await _repo.UpdateAsync(payment);

            var checkoutUrl = _sePayService.GenerateCheckoutUrl();
            var fields = _sePayService.GenerateCheckoutFields(checkoutRequest);

            _logger.LogInformation(
                "Created SePay checkout for payment {PaymentId}, user {UserId}, amount {Amount}",
                payment.Id, userId, amount);

            return new SePayCheckoutResult
            {
                PaymentId = payment.Id,
                CheckoutUrl = checkoutUrl,
                Fields = fields
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SePay checkout: userId={UserId}, amount={Amount}", userId, amount);
            throw;
        }
    }

    public async Task<bool> ProcessSePayCallbackAsync(Dictionary<string, string> webhookData)
    {
        try
        {


            // 2. Extract key fields from webhook data
            var orderInvoiceNumber = webhookData.GetValueOrDefault("order_invoice_number");
            var responseCode = webhookData.GetValueOrDefault("response_code");
            var transactionId = webhookData.GetValueOrDefault("transaction_id");
            var paymentMethod = webhookData.GetValueOrDefault("payment_method");

            if (string.IsNullOrEmpty(orderInvoiceNumber))
            {
                _logger.LogWarning("SePay webhook missing order_invoice_number");
                return false;
            }

            // 3. Look up payment by the order invoice number (which equals payment.Id)
            var payment = await _repo.GetByIdAsync(orderInvoiceNumber);
            if (payment == null)
            {
                _logger.LogWarning("SePay webhook: payment not found for order {OrderInvoiceNumber}", orderInvoiceNumber);
                return false;
            }

            // Prevent double-processing
            if (payment.Status == "Paid")
            {
                _logger.LogInformation("SePay webhook: payment {PaymentId} already processed", payment.Id);
                return true;
            }

            // 4. Update payment with SePay data
            payment.Sepay_TransactionId = transactionId;
            payment.Sepay_ResponseCode = responseCode;
            payment.Sepay_PaymentMethod = paymentMethod;
            payment.Sepay_Signature = webhookData.GetValueOrDefault("signature");

            // SePay returns "00" as success code
            if (responseCode == "00")
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;

                // 5. Credit wallet for TopUp / deposit purposes
                if (payment.Purpose == "TopUp" || payment.Purpose == "Deposit")
                {
                    var (succeeded, errors, _) = await _walletService.ChangeBalanceAsync(
                        payment.UserId, payment.Amount, "Nap tien qua SePay");

                    if (!succeeded)
                    {
                        _logger.LogError(
                            "Failed to credit wallet for SePay payment {PaymentId}. Errors: {Errors}",
                            payment.Id, string.Join(", ", errors));
                    }
                }
            }
            else
            {
                payment.Status = "Failed";
                _logger.LogWarning(
                    "SePay payment {PaymentId} failed with response code {ResponseCode}",
                    payment.Id, responseCode);
            }

            payment.MarkAsUpdated();
            await _repo.UpdateAsync(payment);

            // 6. Send notifications
            await SendPaymentNotificationsAsync(payment);

            _logger.LogInformation(
                "Processed SePay callback for payment {PaymentId}: status={Status}",
                payment.Id, payment.Status);

            return payment.Status == "Paid";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SePay callback");
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

            // Send payment confirmation notifications
            await SendPaymentNotificationsAsync(payment);

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

            // Send payment confirmation notifications
            await SendPaymentNotificationsAsync(payment);

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

    private async Task SendPaymentNotificationsAsync(Payment payment)
    {
        try
        {
            var user = await _userService.GetByIdAsync(payment.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for payment notification: {PaymentId}", payment.Id);
                return;
            }

            var statusText = payment.Status == "Paid" ? "successful" : "failed";
            var paidAt = payment.PaidAt ?? DateTime.UtcNow;

            // Queue email notification
            _backgroundEmailService.QueuePaymentConfirmationEmail(
                user.Email,
                user.FullName,
                payment.Id,
                payment.Amount,
                payment.Currency,
                payment.Status,
                payment.Purpose,
                paidAt);

            // Create in-app notification
            var notification = new Notification
            {
                UserId = payment.UserId,
                Type = "PaymentConfirmation",
                Title = payment.Status == "Paid" ? "Payment Successful" : "Payment Failed",
                Message = $"Your {payment.Purpose} payment of {payment.Amount:N0} {payment.Currency} was {statusText}.",
                Payload = System.Text.Json.JsonSerializer.Serialize(new { PaymentId = payment.Id, Status = payment.Status }),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationService.CreateAsync(notification);

            _logger.LogInformation(
                "Sent payment confirmation notifications for payment {PaymentId}, status: {Status}",
                payment.Id,
                payment.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment notifications for payment {PaymentId}", payment.Id);
            // Don't throw - notification failure shouldn't fail payment processing
        }
    }
}


