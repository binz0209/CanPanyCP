using CanPany.Application.Common.Constants;
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
    private readonly II18nService _i18nService;
    private readonly ILogger<AdminReportsController> _logger;

    public AdminReportsController(
        IReportService reportService,
        II18nService i18nService,
        ILogger<AdminReportsController> logger)
    {
        _reportService = reportService;
        _i18nService = i18nService;
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
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetReportsFailed"));
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
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Report.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report details: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetReportDetailsFailed"));
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
                return Unauthorized(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "Unauthorized"));

            var succeeded = await _reportService.ResolveReportAsync(id, adminId, request.ResolutionNote, request.BanUser);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Report.ResolveFailed), "ResolveFailed"));

            return Ok(ApiResponse.CreateSuccess("Report resolved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving report: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "ResolveReportFailed"));
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
                return Unauthorized(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "Unauthorized"));

            var succeeded = await _reportService.RejectReportAsync(id, adminId, request.RejectionReason);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Report.ResolveFailed), "RejectFailed"));

            return Ok(ApiResponse.CreateSuccess("Report rejected successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting report: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "RejectReportFailed"));
        }
    }
}

public record ResolveReportRequest(string ResolutionNote, bool BanUser);
public record RejectReportRequest(string RejectionReason);
