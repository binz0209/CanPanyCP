using CanPany.Application.DTOs.JobAlerts;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Job Alerts controller
/// </summary>
[ApiController]
[Route("api/job-alerts")]
[Authorize]
public class JobAlertsController : ControllerBase
{
    private readonly IJobAlertService _jobAlertService;
    private readonly ILogger<JobAlertsController> _logger;

    public JobAlertsController(
        IJobAlertService jobAlertService,
        ILogger<JobAlertsController> logger)
    {
        _jobAlertService = jobAlertService;
        _logger = logger;
    }

    /// <summary>
    /// Create new job alert
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] JobAlertCreateDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var alert = await _jobAlertService.CreateAlertAsync(userId, dto);
            return Ok(ApiResponse<JobAlertResponseDto>.CreateSuccess(alert, "Job alert created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job alert");
            return StatusCode(500, ApiResponse.CreateError("Failed to create job alert", "CreateAlertFailed"));
        }
    }

    /// <summary>
    /// Get all job alerts for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAlerts()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var alerts = await _jobAlertService.GetUserAlertsAsync(userId);
            return Ok(ApiResponse<IEnumerable<JobAlertResponseDto>>.CreateSuccess(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job alerts");
            return StatusCode(500, ApiResponse.CreateError("Failed to get job alerts", "GetAlertsFailed"));
        }
    }

    /// <summary>
    /// Get job alert details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlert(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var alert = await _jobAlertService.GetAlertByIdAsync(userId, id);
            if (alert == null)
                return NotFound(ApiResponse.CreateError("Job alert not found", "NotFound"));

            return Ok(ApiResponse<JobAlertResponseDto>.CreateSuccess(alert));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get job alert", "GetAlertFailed"));
        }
    }

    /// <summary>
    /// Update job alert
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlert(string id, [FromBody] JobAlertUpdateDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var alert = await _jobAlertService.UpdateAlertAsync(userId, id, dto);
            if (alert == null)
                return NotFound(ApiResponse.CreateError("Job alert not found", "NotFound"));

            return Ok(ApiResponse<JobAlertResponseDto>.CreateSuccess(alert, "Job alert updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to update job alert", "UpdateAlertFailed"));
        }
    }

    /// <summary>
    /// Delete job alert
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlert(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _jobAlertService.DeleteAlertAsync(userId, id);
            if (!success)
                return NotFound(ApiResponse.CreateError("Job alert not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Job alert deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to delete job alert", "DeleteAlertFailed"));
        }
    }

    /// <summary>
    /// Pause job alert (set IsActive to false)
    /// </summary>
    [HttpPut("{id}/pause")]
    public async Task<IActionResult> PauseAlert(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _jobAlertService.PauseAlertAsync(userId, id);
            if (!success)
                return NotFound(ApiResponse.CreateError("Job alert not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Job alert paused successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing job alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to pause job alert", "PauseAlertFailed"));
        }
    }

    /// <summary>
    /// Resume job alert (set IsActive to true)
    /// </summary>
    [HttpPut("{id}/resume")]
    public async Task<IActionResult> ResumeAlert(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _jobAlertService.ResumeAlertAsync(userId, id);
            if (!success)
                return NotFound(ApiResponse.CreateError("Job alert not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Job alert resumed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming job alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to resume job alert", "ResumeAlertFailed"));
        }
    }

    /// <summary>
    /// Preview matching jobs for alert
    /// </summary>
    [HttpGet("{id}/preview")]
    public async Task<IActionResult> PreviewMatches(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var matches = await _jobAlertService.PreviewMatchesAsync(userId, id);
            return Ok(ApiResponse<IEnumerable<JobMatchInfo>>.CreateSuccess(matches));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing matches for alert {AlertId}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to preview matches", "PreviewFailed"));
        }
    }

    /// <summary>
    /// Get job alert statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var stats = await _jobAlertService.GetStatsAsync(userId);
            return Ok(ApiResponse<object>.CreateSuccess(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job alert stats");
            return StatusCode(500, ApiResponse.CreateError("Failed to get stats", "GetStatsFailed"));
        }
    }
}