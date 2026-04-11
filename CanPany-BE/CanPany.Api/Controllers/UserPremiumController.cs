using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPremiumController : ControllerBase
{
    private readonly IUserPremiumService _userPremiumService;
    private readonly IPremiumPackageService _packageService;

    public UserPremiumController(
        IUserPremiumService userPremiumService,
        IPremiumPackageService packageService)
    {
        _userPremiumService = userPremiumService;
        _packageService = packageService;
    }

    [HttpGet("packages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPackages([FromQuery] string? type)
    {
        try
        {
            var packages = await _packageService.GetAllAsync();
            var activePackages = packages.Where(p => p.IsActive);
            
            if (!string.IsNullOrEmpty(type))
            {
                activePackages = activePackages.Where(p => p.UserType.Equals(type, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(ApiResponse<IEnumerable<PremiumPackage>>.CreateSuccess(activePackages));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError(ex.Message, "GetPackagesFailed"));
        }
    }

    [HttpGet("my-premium")]
    public async Task<IActionResult> GetMyPremium()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var subscription = await _userPremiumService.GetActiveSubscriptionAsync(userId);
            return Ok(ApiResponse<object>.CreateSuccess(new { hasPremium = subscription != null, subscription }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError(ex.Message, "GetMyPremiumFailed"));
        }
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchasePackage([FromBody] PurchasePackageRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (string.IsNullOrEmpty(request.PackageId))
                return BadRequest(ApiResponse.CreateError("Package ID is required", "InvalidRequest"));

            var subscription = await _userPremiumService.PurchasePackageAsync(userId, request.PackageId);
            return Ok(ApiResponse<UserSubscription>.CreateSuccess(subscription, "Mua gói premium thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "PurchaseFailed"));
        }
    }
}

public class PurchasePackageRequest
{
    public string PackageId { get; set; } = string.Empty;
}
