using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Reports controller - User reporting system
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly II18nService _i18nService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        II18nService i18nService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new report
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportDto request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "Unauthorized"));

            var report = await _reportService.CreateReportAsync(userId, request);
            return Ok(ApiResponse.CreateSuccess(report, "Report submitted successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid report request");
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidRequest"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateReportFailed"));
        }
    }

    /// <summary>
    /// Get current user's report history
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyReports()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "Unauthorized"));

            var reports = await _reportService.GetReportsByReporterIdAsync(userId);
            return Ok(ApiResponse.CreateSuccess(reports));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user reports");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetReportsFailed"));
        }
    }

    /// <summary>
    /// Get report details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReport(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.Unauthorized), "Unauthorized"));

            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Report.NotFound), "NotFound"));

            // Verify the report belongs to the current user
            if (report.Reporter.Id != userId)
                return Forbid();

            return Ok(ApiResponse.CreateSuccess(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report: {ReportId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetReportFailed"));
        }
    }
}
