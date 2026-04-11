using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Payments controller - UC-COM-12, UC-COM-13
/// Handles SePay Payment Gateway checkout, premium purchases, and payment history.
/// 
/// NOTE: SePay IPN webhook is handled by SepayController (/api/sepay/ipn).
/// Do NOT add a duplicate webhook endpoint here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly II18nService _i18nService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ISubscriptionService subscriptionService,
        II18nService i18nService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _subscriptionService = subscriptionService;
        _i18nService = i18nService;
        _logger = logger;
    }

    // =========================================================
    // SePay Payment Gateway endpoints
    // =========================================================

    /// <summary>
    /// UC-COM-12: Tạo phiên thanh toán nạp ví qua SePay.
    /// Trả về checkoutUrl + fields để frontend tạo form POST.
    /// </summary>
    [HttpPost("sepay/checkout")]
    public async Task<IActionResult> CreateSePayCheckout([FromBody] CreateSePayCheckoutRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _paymentService.CreateSePayCheckoutAsync(
                userId,
                request.Amount,
                request.Purpose,
                request.PackageId,
                request.SuccessUrl,
                request.ErrorUrl,
                request.CancelUrl);

            return Ok(ApiResponse<object>.CreateSuccess(new
            {
                paymentId = result.PaymentId,
                checkoutUrl = result.CheckoutUrl,
                fields = result.Fields
            }, "Checkout session created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SePay checkout");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.CreateFailed), "CreateCheckoutFailed"));
        }
    }

    // =========================================================
    // Premium subscription endpoints
    // =========================================================

    /// <summary>
    /// UC-COM-13: Mua gói Premium bằng số dư ví.
    /// Trừ tiền ví → tạo Payment (Paid) → tạo UserSubscription (Active).
    /// </summary>
    [HttpPost("premium/purchase")]
    public async Task<IActionResult> PurchasePremiumWithWallet([FromBody] PurchasePremiumWalletRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (succeeded, errors, subscription) = await _subscriptionService.PurchasePremiumAsync(userId, request.PackageId);

            if (!succeeded)
                return BadRequest(ApiResponse.CreateError(string.Join("; ", errors), "PurchaseFailed"));

            return Ok(ApiResponse<object>.CreateSuccess(new
            {
                subscriptionId = subscription!.Id,
                startDate = subscription.StartDate,
                endDate = subscription.EndDate,
                status = subscription.Status,
                features = subscription.Features
            }, "Premium đã được kích hoạt thành công!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing premium");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.CreateFailed), "PurchasePremiumFailed"));
        }
    }

    /// <summary>
    /// Get current active premium subscription
    /// </summary>
    [HttpGet("premium/status")]
    public async Task<IActionResult> GetPremiumStatus()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            var isPremium = subscription != null;

            return Ok(ApiResponse<object>.CreateSuccess(new
            {
                isPremium,
                subscription = subscription != null ? new
                {
                    id = subscription.Id,
                    packageId = subscription.PackageId,
                    status = subscription.Status,
                    startDate = subscription.StartDate,
                    endDate = subscription.EndDate,
                    features = subscription.Features
                } : null
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium status");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPremiumStatusFailed"));
        }
    }

    /// <summary>
    /// Get available premium packages
    /// </summary>
    [HttpGet("premium/packages")]
    public async Task<IActionResult> GetPremiumPackages()
    {
        try
        {
            var packages = await _subscriptionService.GetAvailablePackagesAsync();
            return Ok(ApiResponse.CreateSuccess(packages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium packages");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPackagesFailed"));
        }
    }

    // =========================================================
    // Payment history
    // =========================================================

    /// <summary>
    /// Get payment history
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var payments = await _paymentService.GetByUserIdAsync(userId);
            return Ok(ApiResponse.CreateSuccess(payments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPaymentsFailed"));
        }
    }

    // =========================================================
    // Legacy endpoints (kept for backward compatibility)
    // =========================================================

    /// <summary>
    /// Legacy: Create Deposit Request (manual approval flow)
    /// </summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var payment = await _paymentService.CreateDepositRequestAsync(userId, (long)request.Amount);
            return Ok(ApiResponse<Payment>.CreateSuccess(payment, "Deposit request created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deposit request");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.CreateFailed), "CreateDepositFailed"));
        }
    }

    /// <summary>
    /// Legacy payment callback (VNPay - backward compatibility)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromForm] Dictionary<string, string> paymentData)
    {
        try
        {
            var paymentId = paymentData.GetValueOrDefault("vnp_TxnRef");
            if (string.IsNullOrEmpty(paymentId))
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "InvalidPaymentData"));

            var succeeded = await _paymentService.ProcessPaymentAsync(paymentId, paymentData);
            if (succeeded)
                return Ok(ApiResponse.CreateSuccess("Payment processed successfully"));

            return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.CreateFailed), "PaymentProcessingFailed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "PaymentCallbackFailed"));
        }
    }
}

// =========================================================
// Request DTOs
// =========================================================

public record CreateSePayCheckoutRequest(
    long Amount,
    string Purpose = "TopUp",
    string? PackageId = null,
    string SuccessUrl = "",
    string ErrorUrl = "",
    string CancelUrl = "");

public record PurchasePremiumWalletRequest(string PackageId);
public record CreateDepositRequest(decimal Amount);
