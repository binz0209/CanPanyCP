using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Api.Controllers;

/// <summary>
/// Applications controller - UC-CAN-18 to UC-CAN-22, UC-CMP-14 to UC-CMP-18
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IJobService _jobService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IApplicationService applicationService,
        IJobService jobService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-18: Submit Job Application (Proposal)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if already applied
            var hasApplied = await _applicationService.HasAppliedAsync(request.JobId, userId);
            if (hasApplied)
                return BadRequest(ApiResponse.CreateError("You have already applied for this job", "AlreadyApplied"));

            var application = new DomainApplication
            {
                JobId = request.JobId,
                CandidateId = userId,
                CVId = request.CVId,
                CoverLetter = request.CoverLetter,
                ExpectedSalary = request.ExpectedSalary,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _applicationService.CreateAsync(application);
            return Ok(ApiResponse<DomainApplication>.CreateSuccess(created, "Application submitted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application");
            return StatusCode(500, ApiResponse.CreateError("Failed to submit application", "CreateApplicationFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-20: View Application History
    /// </summary>
    [HttpGet("my-applications")]
    public async Task<IActionResult> GetMyApplications()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var applications = await _applicationService.GetByCandidateIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<DomainApplication>>.CreateSuccess(applications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applications");
            return StatusCode(500, ApiResponse.CreateError("Failed to get applications", "GetApplicationsFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-21: View Application Status
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApplication(string id)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError("Application not found", "NotFound"));

            return Ok(ApiResponse<DomainApplication>.CreateSuccess(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application");
            return StatusCode(500, ApiResponse.CreateError("Failed to get application", "GetApplicationFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-22: Withdraw Application
    /// </summary>
    [HttpPut("{id}/withdraw")]
    public async Task<IActionResult> WithdrawApplication(string id)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError("Application not found", "NotFound"));

            application.Status = "Withdrawn";
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application withdrawn successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing application");
            return StatusCode(500, ApiResponse.CreateError("Failed to withdraw application", "WithdrawApplicationFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-14: View Application List (per Job)
    /// </summary>
    [HttpGet("job/{jobId}")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> GetJobApplications(string jobId)
    {
        try
        {
            var applications = await _applicationService.GetByJobIdAsync(jobId);
            return Ok(ApiResponse<IEnumerable<DomainApplication>>.CreateSuccess(applications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job applications");
            return StatusCode(500, ApiResponse.CreateError("Failed to get job applications", "GetJobApplicationsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-15: View Application Details
    /// </summary>
    [HttpGet("{id}/details")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> GetApplicationDetails(string id)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError("Application not found", "NotFound"));

            return Ok(ApiResponse<DomainApplication>.CreateSuccess(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application details");
            return StatusCode(500, ApiResponse.CreateError("Failed to get application details", "GetApplicationDetailsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-16: Accept Application (Shortlist/Interview)
    /// </summary>
    [HttpPut("{id}/accept")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> AcceptApplication(string id)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError("Application not found", "NotFound"));

            application.Status = "Accepted";
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application accepted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting application");
            return StatusCode(500, ApiResponse.CreateError("Failed to accept application", "AcceptApplicationFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-17: Reject Application
    /// </summary>
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> RejectApplication(string id, [FromBody] RejectApplicationRequest request)
    {
        try
        {
            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError("Application not found", "NotFound"));

            application.Status = "Rejected";
            application.RejectedReason = request.Reason;
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application rejected successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting application");
            return StatusCode(500, ApiResponse.CreateError("Failed to reject application", "RejectApplicationFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-18: Add Private Note to Application
    /// </summary>
    [HttpPut("{id}/note")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> AddNote(string id, [FromBody] AddNoteRequest request)
    {
        try
        {
            // TODO: Implement private notes feature
            // This might require a separate Notes entity or field in Application
            await Task.CompletedTask;
            return Ok(ApiResponse.CreateSuccess("Note added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note");
            return StatusCode(500, ApiResponse.CreateError("Failed to add note", "AddNoteFailed"));
        }
    }
}

public record CreateApplicationRequest(
    string JobId,
    string? CVId = null,
    string? CoverLetter = null,
    decimal? ExpectedSalary = null
);

public record RejectApplicationRequest(string Reason);
public record AddNoteRequest(string Note);

