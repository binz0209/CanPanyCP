using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainApplication = CanPany.Domain.Entities.Application;

using CanPany.Application.DTOs.Applications;

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
    private readonly ICompanyService _companyService;
    private readonly II18nService _i18nService;
    private readonly ILogger<ApplicationsController> _logger;

    // Valid status transitions: from -> allowed to
    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        ["Pending"]     = new() { "Shortlisted", "Accepted", "Rejected", "Withdrawn" },
        ["Shortlisted"] = new() { "Accepted", "Rejected", "Withdrawn" },
        ["Accepted"]    = new(),  // terminal
        ["Rejected"]    = new(),  // terminal
        ["Withdrawn"]   = new(),  // terminal
    };

    public ApplicationsController(
        IApplicationService applicationService,
        IJobService jobService,
        ICompanyService companyService,
        II18nService i18nService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _jobService = jobService;
        _companyService = companyService;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// Check if a status transition is valid.
    /// </summary>
    private static bool IsValidTransition(string currentStatus, string newStatus)
    {
        return ValidTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus);
    }

    /// <summary>
    /// Check if the current Company user owns the job that the application belongs to.
    /// </summary>
    private async Task<bool> IsCompanyOwnerOfApplicationAsync(string userId, DomainApplication application)
    {
        var company = await _companyService.GetByUserIdAsync(userId);
        if (company == null) return false;
        var job = await _jobService.GetByIdAsync(application.JobId);
        return job != null && job.CompanyId == company.Id;
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
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "AlreadyApplied"));

            var application = new DomainApplication
            {
                JobId = request.JobId,
                CandidateId = userId,
                CVId = request.CVId,
                CoverLetter = request.CoverLetter,
                ProposedAmount = request.ExpectedSalary,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _applicationService.CreateAsync(application);
            return Ok(ApiResponse<DomainApplication>.CreateSuccess(created, _i18nService.GetDisplayMessage(I18nKeys.Success.Application.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateApplicationFailed"));
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
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetApplicationsFailed"));
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
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            return Ok(ApiResponse<DomainApplication>.CreateSuccess(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetApplicationFailed"));
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
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            // Ownership check: only the candidate who applied can withdraw
            if (application.CandidateId != userId)
                return Forbid();

            // State machine check
            if (!IsValidTransition(application.Status, "Withdrawn"))
                return BadRequest(ApiResponse.CreateError($"Cannot withdraw an application with status '{application.Status}'.", "InvalidStatusTransition"));

            application.Status = "Withdrawn";
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application withdrawn successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing application");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.UpdateFailed), "WithdrawApplicationFailed"));
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
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetJobApplicationsFailed"));
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
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            return Ok(ApiResponse<DomainApplication>.CreateSuccess(application));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application details");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetApplicationDetailsFailed"));
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
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            // Ownership check: company must own the job
            var userRole = User.FindFirst("role")?.Value;
            if (userRole != "Admin" && !await IsCompanyOwnerOfApplicationAsync(userId, application))
                return Forbid();

            // State machine check
            if (!IsValidTransition(application.Status, "Accepted"))
                return BadRequest(ApiResponse.CreateError($"Cannot accept an application with status '{application.Status}'.", "InvalidStatusTransition"));

            application.Status = "Accepted";
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application accepted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting application");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.UpdateFailed), "AcceptApplicationFailed"));
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
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            // Ownership check: company must own the job
            var userRole = User.FindFirst("role")?.Value;
            if (userRole != "Admin" && !await IsCompanyOwnerOfApplicationAsync(userId, application))
                return Forbid();

            // State machine check
            if (!IsValidTransition(application.Status, "Rejected"))
                return BadRequest(ApiResponse.CreateError($"Cannot reject an application with status '{application.Status}'.", "InvalidStatusTransition"));

            application.Status = "Rejected";
            application.RejectedReason = request.Reason;
            await _applicationService.UpdateAsync(id, application);
            return Ok(ApiResponse.CreateSuccess("Application rejected successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting application");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.UpdateFailed), "RejectApplicationFailed"));
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
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "InvalidApplicationId"));

            if (request == null || string.IsNullOrWhiteSpace(request.Note))
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NoteRequired), "InvalidNote"));

            var application = await _applicationService.GetByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.NotFound), "NotFound"));

            var trimmedNote = request.Note.Trim();
            application.PrivateNotes = string.IsNullOrWhiteSpace(application.PrivateNotes)
                ? trimmedNote
                : $"{application.PrivateNotes}\n{trimmedNote}";

            var updated = await _applicationService.UpdateAsync(id, application);
            if (!updated)
                return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Application.UpdateFailed), "AddNoteFailed"));

            return Ok(ApiResponse.CreateSuccess("Note added successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "AddNoteFailed"));
        }
    }
}



