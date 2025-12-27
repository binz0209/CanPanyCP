using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Jobs controller - UC-CAN-13 to UC-CAN-17, UC-CMP-04 to UC-CMP-11
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IBookmarkService _bookmarkService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        IBookmarkService bookmarkService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _bookmarkService = bookmarkService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-13: Search Jobs
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchJobs([FromQuery] string? keyword, [FromQuery] string? categoryId, [FromQuery] List<string>? skillIds, [FromQuery] decimal? minBudget, [FromQuery] decimal? maxBudget)
    {
        try
        {
            var jobs = await _jobService.SearchAsync(keyword, categoryId, skillIds, minBudget, maxBudget);
            return Ok(ApiResponse<IEnumerable<Job>>.CreateSuccess(jobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to search jobs", "SearchJobsFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-14: View Job Details
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetJob(string id)
    {
        try
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job == null)
                return NotFound(ApiResponse.CreateError("Job not found", "NotFound"));

            // Check if bookmarked (if user is authenticated)
            bool isBookmarked = false;
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, id);
            }

            return Ok(ApiResponse<object>.CreateSuccess(new { job, isBookmarked }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job");
            return StatusCode(500, ApiResponse.CreateError("Failed to get job", "GetJobFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-15: View AI-Recommended Jobs
    /// </summary>
    [HttpGet("recommended")]
    [Authorize]
    public async Task<IActionResult> GetRecommendedJobs([FromQuery] int limit = 10)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // TODO: Implement AI recommendation logic
            // This should use vector search and matching algorithms
            // For now, return empty list
            return Ok(ApiResponse<IEnumerable<Job>>.CreateSuccess(new List<Job>()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get recommended jobs", "GetRecommendedJobsFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-16: Bookmark Job
    /// </summary>
    [HttpPost("{id}/bookmark")]
    [Authorize]
    public async Task<IActionResult> BookmarkJob(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _bookmarkService.BookmarkJobAsync(userId, id);
            return Ok(ApiResponse.CreateSuccess("Job bookmarked successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bookmarking job");
            return StatusCode(500, ApiResponse.CreateError("Failed to bookmark job", "BookmarkJobFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-17: Remove Bookmarked Job
    /// </summary>
    [HttpDelete("{id}/bookmark")]
    [Authorize]
    public async Task<IActionResult> RemoveBookmark(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _bookmarkService.RemoveBookmarkAsync(userId, id);
            return Ok(ApiResponse.CreateSuccess("Bookmark removed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing bookmark");
            return StatusCode(500, ApiResponse.CreateError("Failed to remove bookmark", "RemoveBookmarkFailed"));
        }
    }

    /// <summary>
    /// Get bookmarked jobs
    /// </summary>
    [HttpGet("bookmarked")]
    [Authorize]
    public async Task<IActionResult> GetBookmarkedJobs()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var jobs = await _bookmarkService.GetBookmarkedJobsAsync(userId);
            return Ok(ApiResponse<IEnumerable<Job>>.CreateSuccess(jobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookmarked jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get bookmarked jobs", "GetBookmarkedJobsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-04: Create Job
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var job = new Job
            {
                CompanyId = request.CompanyId,
                Title = request.Title,
                Description = request.Description,
                CategoryId = request.CategoryId,
                SkillIds = request.SkillIds ?? new List<string>(),
                BudgetType = request.BudgetType,
                BudgetAmount = request.BudgetAmount,
                Level = request.Level,
                Location = request.Location,
                IsRemote = request.IsRemote,
                Deadline = request.Deadline,
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _jobService.CreateAsync(job);
            return Ok(ApiResponse<Job>.CreateSuccess(created, "Job created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, ApiResponse.CreateError("Failed to create job", "CreateJobFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-06: View Job List (for company)
    /// </summary>
    [HttpGet("company/{companyId}")]
    [Authorize]
    public async Task<IActionResult> GetCompanyJobs(string companyId)
    {
        try
        {
            var jobs = await _jobService.GetByCompanyIdAsync(companyId);
            return Ok(ApiResponse<IEnumerable<Job>>.CreateSuccess(jobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get company jobs", "GetCompanyJobsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-08: Update Job Content
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> UpdateJob(string id, [FromBody] UpdateJobRequest request)
    {
        try
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job == null)
                return NotFound(ApiResponse.CreateError("Job not found", "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.Title)) job.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Description)) job.Description = request.Description;
            if (request.SkillIds != null) job.SkillIds = request.SkillIds;
            if (request.BudgetAmount.HasValue) job.BudgetAmount = request.BudgetAmount;
            if (!string.IsNullOrWhiteSpace(request.Level)) job.Level = request.Level;
            if (!string.IsNullOrWhiteSpace(request.Location)) job.Location = request.Location;
            if (request.Deadline.HasValue) job.Deadline = request.Deadline;

            await _jobService.UpdateAsync(id, job);
            return Ok(ApiResponse.CreateSuccess("Job updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job");
            return StatusCode(500, ApiResponse.CreateError("Failed to update job", "UpdateJobFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-09: Close Job
    /// </summary>
    [HttpPut("{id}/close")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> CloseJob(string id)
    {
        try
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job == null)
                return NotFound(ApiResponse.CreateError("Job not found", "NotFound"));

            job.Status = "Closed";
            await _jobService.UpdateAsync(id, job);
            return Ok(ApiResponse.CreateSuccess("Job closed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing job");
            return StatusCode(500, ApiResponse.CreateError("Failed to close job", "CloseJobFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-10: Re-open Job
    /// </summary>
    [HttpPut("{id}/reopen")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> ReopenJob(string id)
    {
        try
        {
            var job = await _jobService.GetByIdAsync(id);
            if (job == null)
                return NotFound(ApiResponse.CreateError("Job not found", "NotFound"));

            job.Status = "Open";
            await _jobService.UpdateAsync(id, job);
            return Ok(ApiResponse.CreateSuccess("Job reopened successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening job");
            return StatusCode(500, ApiResponse.CreateError("Failed to reopen job", "ReopenJobFailed"));
        }
    }
}

public record CreateJobRequest(
    string CompanyId,
    string Title,
    string Description,
    string? CategoryId = null,
    List<string>? SkillIds = null,
    string BudgetType = "Fixed",
    decimal? BudgetAmount = null,
    string? Level = null,
    string? Location = null,
    bool IsRemote = false,
    DateTime? Deadline = null
);

public record UpdateJobRequest(
    string? Title = null,
    string? Description = null,
    List<string>? SkillIds = null,
    decimal? BudgetAmount = null,
    string? Level = null,
    string? Location = null,
    DateTime? Deadline = null
);

