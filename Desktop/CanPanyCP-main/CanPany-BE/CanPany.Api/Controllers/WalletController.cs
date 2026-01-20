using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Wallet controller - UC-COM-10, UC-COM-11, UC-COM-12
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(
        IWalletService walletService,
        ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    /// <summary>
    /// UC-COM-10: View Wallet Balance
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var balance = await _walletService.GetBalanceAsync(userId);
            var wallet = await _walletService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<object>.CreateSuccess(new { balance, wallet }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet balance");
            return StatusCode(500, ApiResponse.CreateError("Failed to get wallet balance", "GetBalanceFailed"));
        }
    }

    /// <summary>
    /// UC-COM-11: View Transaction History
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int take = 20)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var transactions = await _walletService.GetTransactionHistoryAsync(userId, take);
            return Ok(ApiResponse<IEnumerable<WalletTransaction>>.CreateSuccess(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history");
            return StatusCode(500, ApiResponse.CreateError("Failed to get transaction history", "GetTransactionsFailed"));
        }
    }
}


