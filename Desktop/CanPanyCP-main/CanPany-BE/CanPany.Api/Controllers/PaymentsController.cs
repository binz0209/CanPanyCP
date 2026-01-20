using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Payments controller - UC-COM-12, UC-COM-13
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// UC-COM-12: Create Deposit Request
    /// </summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> CreateDeposit([FromBody] CreateDepositRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Convert VND to minor units (multiply by 100)
            var amountInMinorUnits = (long)(request.Amount * 100);
            var payment = await _paymentService.CreateDepositRequestAsync(userId, amountInMinorUnits);
            return Ok(ApiResponse<Payment>.CreateSuccess(payment, "Deposit request created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating deposit request");
            return StatusCode(500, ApiResponse.CreateError("Failed to create deposit request", "CreateDepositFailed"));
        }
    }

    /// <summary>
    /// UC-COM-13: Purchase Premium Package
    /// </summary>
    [HttpPost("premium")]
    public async Task<IActionResult> PurchasePremium([FromBody] PurchasePremiumRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var amountInMinorUnits = (long)(request.Amount * 100);
            var payment = await _paymentService.CreatePremiumPurchaseAsync(userId, request.PackageId, amountInMinorUnits);
            return Ok(ApiResponse<Payment>.CreateSuccess(payment, "Premium purchase request created"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premium purchase");
            return StatusCode(500, ApiResponse.CreateError("Failed to create premium purchase", "PurchasePremiumFailed"));
        }
    }

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
            return StatusCode(500, ApiResponse.CreateError("Failed to get payments", "GetPaymentsFailed"));
        }
    }

    /// <summary>
    /// Payment callback from SePay/VNPay
    /// </summary>
    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<IActionResult> PaymentCallback([FromForm] Dictionary<string, string> paymentData)
    {
        try
        {
            var paymentId = paymentData.GetValueOrDefault("vnp_TxnRef");
            if (string.IsNullOrEmpty(paymentId))
                return BadRequest(ApiResponse.CreateError("Invalid payment data", "InvalidPaymentData"));

            var succeeded = await _paymentService.ProcessPaymentAsync(paymentId, paymentData);
            if (succeeded)
            {
                return Ok(ApiResponse.CreateSuccess("Payment processed successfully"));
            }

            return BadRequest(ApiResponse.CreateError("Payment processing failed", "PaymentProcessingFailed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback");
            return StatusCode(500, ApiResponse.CreateError("Failed to process payment", "PaymentCallbackFailed"));
        }
    }
}

public record CreateDepositRequest(decimal Amount);
public record PurchasePremiumRequest(string PackageId, decimal Amount);


