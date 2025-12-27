using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Admin controller - UC-ADM-01 to UC-ADM-26
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IJobService _jobService;
    private readonly IPaymentService _paymentService;
    private readonly ICategoryService _categoryService;
    private readonly ISkillService _skillService;
    private readonly IBannerService _bannerService;
    private readonly IPremiumPackageService _premiumPackageService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminDashboardService dashboardService,
        IAdminService adminService,
        IUserService userService,
        ICompanyService companyService,
        IJobService jobService,
        IPaymentService paymentService,
        ICategoryService categoryService,
        ISkillService skillService,
        IBannerService bannerService,
        IPremiumPackageService premiumPackageService,
        ILogger<AdminController> logger)
    {
        _dashboardService = dashboardService;
        _adminService = adminService;
        _userService = userService;
        _companyService = companyService;
        _jobService = jobService;
        _paymentService = paymentService;
        _categoryService = categoryService;
        _skillService = skillService;
        _bannerService = bannerService;
        _premiumPackageService = premiumPackageService;
        _logger = logger;
    }

    /// <summary>
    /// UC-ADM-01: View System Dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(ApiResponse<object>.CreateSuccess(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, ApiResponse.CreateError("Failed to get dashboard stats", "GetDashboardFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-02: View User List
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<User>>.CreateSuccess(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, ApiResponse.CreateError("Failed to get users", "GetUsersFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-05: Ban User
    /// </summary>
    [HttpPut("users/{id}/ban")]
    public async Task<IActionResult> BanUser(string id)
    {
        try
        {
            var succeeded = await _adminService.BanUserAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("User not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("User banned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user");
            return StatusCode(500, ApiResponse.CreateError("Failed to ban user", "BanUserFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-06: Unban User
    /// </summary>
    [HttpPut("users/{id}/unban")]
    public async Task<IActionResult> UnbanUser(string id)
    {
        try
        {
            var succeeded = await _adminService.UnbanUserAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("User not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("User unbanned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user");
            return StatusCode(500, ApiResponse.CreateError("Failed to unban user", "UnbanUserFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-07: View Company Verification Requests
    /// </summary>
    [HttpGet("companies/verification-requests")]
    public async Task<IActionResult> GetVerificationRequests()
    {
        try
        {
            // TODO: Implement get verification requests
            return Ok(ApiResponse.CreateSuccess(new List<object>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification requests");
            return StatusCode(500, ApiResponse.CreateError("Failed to get verification requests", "GetVerificationRequestsFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-08: Approve Company Verification
    /// </summary>
    [HttpPut("companies/{id}/verify/approve")]
    public async Task<IActionResult> ApproveVerification(string id)
    {
        try
        {
            var succeeded = await _adminService.ApproveCompanyVerificationAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Company verification approved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving verification");
            return StatusCode(500, ApiResponse.CreateError("Failed to approve verification", "ApproveVerificationFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-09: Reject Company Verification
    /// </summary>
    [HttpPut("companies/{id}/verify/reject")]
    public async Task<IActionResult> RejectVerification(string id, [FromBody] RejectVerificationRequest request)
    {
        try
        {
            var succeeded = await _adminService.RejectCompanyVerificationAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Company verification rejected"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting verification");
            return StatusCode(500, ApiResponse.CreateError("Failed to reject verification", "RejectVerificationFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-11: Hide Job (Violation)
    /// </summary>
    [HttpPut("jobs/{id}/hide")]
    public async Task<IActionResult> HideJob(string id, [FromBody] HideJobRequest request)
    {
        try
        {
            var succeeded = await _adminService.HideJobAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Job not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Job hidden successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding job");
            return StatusCode(500, ApiResponse.CreateError("Failed to hide job", "HideJobFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-12: Delete Job
    /// </summary>
    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(string id)
    {
        try
        {
            await _adminService.DeleteJobAsync(id);
            return Ok(ApiResponse.CreateSuccess("Job deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete job", "DeleteJobFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-13: Create Category
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = new Category { Name = request.Name };
            var created = await _categoryService.CreateAsync(category);
            return Ok(ApiResponse.CreateSuccess(created, "Category created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, ApiResponse.CreateError("Failed to create category", "CreateCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-14: Update Category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse.CreateError("Category not found", "NotFound"));

            category.Name = request.Name;
            await _categoryService.UpdateAsync(id, category);
            return Ok(ApiResponse.CreateSuccess("Category updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, ApiResponse.CreateError("Failed to update category", "UpdateCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-15: Delete Category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Category deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete category", "DeleteCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-16: Create Skill
    /// </summary>
    [HttpPost("skills")]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request)
    {
        try
        {
            var skill = new Skill { Name = request.Name, CategoryId = request.CategoryId };
            var created = await _skillService.CreateAsync(skill);
            return Ok(ApiResponse.CreateSuccess(created, "Skill created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            return StatusCode(500, ApiResponse.CreateError("Failed to create skill", "CreateSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-17: Update Skill
    /// </summary>
    [HttpPut("skills/{id}")]
    public async Task<IActionResult> UpdateSkill(string id, [FromBody] UpdateSkillRequest request)
    {
        try
        {
            var skill = await _skillService.GetByIdAsync(id);
            if (skill == null)
                return NotFound(ApiResponse.CreateError("Skill not found", "NotFound"));

            skill.Name = request.Name;
            skill.CategoryId = request.CategoryId;
            await _skillService.UpdateAsync(id, skill);
            return Ok(ApiResponse.CreateSuccess("Skill updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill");
            return StatusCode(500, ApiResponse.CreateError("Failed to update skill", "UpdateSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-18: Delete Skill
    /// </summary>
    [HttpDelete("skills/{id}")]
    public async Task<IActionResult> DeleteSkill(string id)
    {
        try
        {
            await _skillService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Skill deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete skill", "DeleteSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-19: Create Banner
    /// </summary>
    [HttpPost("banners")]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerRequest request)
    {
        try
        {
            var banner = new Banner
            {
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                LinkUrl = request.LinkUrl,
                Order = request.Order,
                IsActive = request.IsActive ?? true
            };
            var created = await _bannerService.CreateAsync(banner);
            return Ok(ApiResponse.CreateSuccess(created, "Banner created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating banner");
            return StatusCode(500, ApiResponse.CreateError("Failed to create banner", "CreateBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-20: Update Banner
    /// </summary>
    [HttpPut("banners/{id}")]
    public async Task<IActionResult> UpdateBanner(string id, [FromBody] UpdateBannerRequest request)
    {
        try
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null)
                return NotFound(ApiResponse.CreateError("Banner not found", "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.Title)) banner.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.ImageUrl)) banner.ImageUrl = request.ImageUrl;
            if (request.LinkUrl != null) banner.LinkUrl = request.LinkUrl;
            if (request.Order.HasValue) banner.Order = request.Order.Value;
            if (request.IsActive.HasValue) banner.IsActive = request.IsActive.Value;

            await _bannerService.UpdateAsync(id, banner);
            return Ok(ApiResponse.CreateSuccess("Banner updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banner");
            return StatusCode(500, ApiResponse.CreateError("Failed to update banner", "UpdateBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-21: Delete Banner
    /// </summary>
    [HttpDelete("banners/{id}")]
    public async Task<IActionResult> DeleteBanner(string id)
    {
        try
        {
            await _bannerService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Banner deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting banner");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete banner", "DeleteBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-22: Update Premium Package Price
    /// </summary>
    [HttpPut("premium-packages/{id}/price")]
    public async Task<IActionResult> UpdatePackagePrice(string id, [FromBody] UpdatePriceRequest request)
    {
        try
        {
            // Convert VND to minor units
            var priceInMinorUnits = (long)(request.Price * 100);
            var succeeded = await _premiumPackageService.UpdatePriceAsync(id, priceInMinorUnits);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Package not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Package price updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package price");
            return StatusCode(500, ApiResponse.CreateError("Failed to update package price", "UpdatePriceFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-23: View Payment Requests
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] string? status = null)
    {
        try
        {
            // TODO: Get all payments or filter by status
            return Ok(ApiResponse.CreateSuccess(new List<object>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, ApiResponse.CreateError("Failed to get payments", "GetPaymentsFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-24: Approve/Reject Payment Request
    /// </summary>
    [HttpPut("payments/{id}/approve")]
    public async Task<IActionResult> ApprovePayment(string id)
    {
        try
        {
            var succeeded = await _adminService.ApprovePaymentRequestAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Payment not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Payment approved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payment");
            return StatusCode(500, ApiResponse.CreateError("Failed to approve payment", "ApprovePaymentFailed"));
        }
    }

    [HttpPut("payments/{id}/reject")]
    public async Task<IActionResult> RejectPayment(string id, [FromBody] RejectPaymentRequest request)
    {
        try
        {
            var succeeded = await _adminService.RejectPaymentRequestAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Payment not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Payment rejected successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment");
            return StatusCode(500, ApiResponse.CreateError("Failed to reject payment", "RejectPaymentFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-25: View Audit Logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? userId = null, [FromQuery] string? entityType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var logs = await _adminService.GetAuditLogsAsync(userId, entityType, fromDate, toDate);
            return Ok(ApiResponse.CreateSuccess(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get audit logs", "GetAuditLogsFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-26: Send Broadcast Notification
    /// </summary>
    [HttpPost("notifications/broadcast")]
    public async Task<IActionResult> SendBroadcast([FromBody] BroadcastNotificationRequest request)
    {
        try
        {
            var succeeded = await _adminService.SendBroadcastNotificationAsync(request.Title, request.Message, request.TargetRole);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError("Failed to send broadcast notification", "BroadcastFailed"));

            return Ok(ApiResponse.CreateSuccess("Broadcast notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending broadcast notification");
            return StatusCode(500, ApiResponse.CreateError("Failed to send broadcast notification", "BroadcastFailed"));
        }
    }
}

public record RejectVerificationRequest(string Reason);
public record HideJobRequest(string Reason);
public record CreateCategoryRequest(string Name);
public record UpdateCategoryRequest(string Name);
public record CreateSkillRequest(string Name, string? CategoryId = null);
public record UpdateSkillRequest(string Name, string? CategoryId = null);
public record CreateBannerRequest(string Title, string ImageUrl, string? LinkUrl = null, int Order = 0, bool? IsActive = true);
public record UpdateBannerRequest(string? Title = null, string? ImageUrl = null, string? LinkUrl = null, int? Order = null, bool? IsActive = null);
public record UpdatePriceRequest(decimal Price);
public record RejectPaymentRequest(string Reason);
public record BroadcastNotificationRequest(string Title, string Message, string? TargetRole = null);


