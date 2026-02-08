using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Admin reports controller - Admin reporting management
/// </summary>
[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<AdminReportsController> _logger;

    public AdminReportsController(
        IReportService reportService,
        ILogger<AdminReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reports (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllReports([FromQuery] ReportFilterDto filter)
    {
        try
        {
            var reports = await _reportService.GetAllReportsAsync(filter);
            return Ok(ApiResponse.CreateSuccess(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all reports");
            return StatusCode(500, ApiResponse.CreateError("Failed to get reports", "GetReportsFailed"));
        }
    }

    /// <summary>
    /// Get report details with evidence (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReportDetails(string id)
    {
        try
        {
            var report = await _reportService.GetReportDetailsAsync(id);
            if (report == null)
                return NotFound(ApiResponse.CreateError("Report not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report details: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get report details", "GetReportDetailsFailed"));
        }
    }

    /// <summary>
    /// Resolve report (Admin only)
    /// </summary>
    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> ResolveReport(string id, [FromBody] ResolveReportRequest request)
    {
        try
        {
            var adminId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(ApiResponse.CreateError("Admin not authenticated", "Unauthorized"));

            var succeeded = await _reportService.ResolveReportAsync(id, adminId, request.ResolutionNote, request.BanUser);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError("Failed to resolve report or report not in Pending status", "ResolveFailed"));

            return Ok(ApiResponse.CreateSuccess("Report resolved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving report: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to resolve report", "ResolveReportFailed"));
        }
    }

    /// <summary>
    /// Reject report (Admin only)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectReport(string id, [FromBody] RejectReportRequest request)
    {
        try
        {
            var adminId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(ApiResponse.CreateError("Admin not authenticated", "Unauthorized"));

            var succeeded = await _reportService.RejectReportAsync(id, adminId, request.RejectionReason);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError("Failed to reject report or report not in Pending status", "RejectFailed"));

            return Ok(ApiResponse.CreateSuccess("Report rejected successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to reject report", "RejectReportFailed"));
        }
    }
}

public record ResolveReportRequest(string ResolutionNote, bool BanUser);
public record RejectReportRequest(string RejectionReason);
