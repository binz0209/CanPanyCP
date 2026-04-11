using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanPany.Worker.Infrastructure.Progress;
using CanPany.Worker.Infrastructure.Queue;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using System.Text.Json;

using CanPany.Application.DTOs.Jobs;
using System.Text.Json.Serialization;

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
    private readonly IJobMatchingService _jobMatchingService;
    private readonly IHybridRecommendationService _recommendationService;
    private readonly IInteractionTrackingService _interactionService;
    private readonly IJobProducer _jobProducer;
    private readonly IJobProgressTracker _progressTracker;
    private readonly II18nService _i18nService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        IBookmarkService bookmarkService,
        IJobMatchingService jobMatchingService,
        IHybridRecommendationService recommendationService,
        IInteractionTrackingService interactionService,
        IJobProducer jobProducer,
        IJobProgressTracker progressTracker,
        II18nService i18nService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _bookmarkService = bookmarkService;
        _jobMatchingService = jobMatchingService;
        _recommendationService = recommendationService;
        _interactionService = interactionService;
        _jobProducer = jobProducer;
        _progressTracker = progressTracker;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-13: Search Jobs
    /// If user is authenticated, results are sorted by hybrid score (relevance)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchJobs([FromQuery] string? keyword, [FromQuery] string? categoryId, [FromQuery] List<string>? skillIds, [FromQuery] decimal? minBudget, [FromQuery] decimal? maxBudget)
    {
        try
        {
            var jobs = (await _jobService.SearchAsync(keyword, categoryId, skillIds, minBudget, maxBudget)).ToList();
            
            // If user is authenticated, calculate scores and sort by relevance
            var userId = User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId) && jobs.Any())
            {
                try
                {
                    var scores = await _recommendationService.CalculateScoresForJobsAsync(userId, jobs);
                    
                    // Sort jobs by score (descending), then by createdAt (descending) for same scores
                    jobs = jobs
                        .OrderByDescending(j => scores.TryGetValue(j.Id, out var score) ? score : 0)
                        .ThenByDescending(j => j.CreatedAt)
                        .ToList();
                    
                    _logger.LogDebug(
                        "Sorted {JobCount} jobs by hybrid score for user {UserId}. Score range: {MinScore:F2} - {MaxScore:F2}",
                        jobs.Count, userId,
                        scores.Values.Any() ? scores.Values.Min() : 0,
                        scores.Values.Any() ? scores.Values.Max() : 0);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to calculate scores for search results, returning unsorted");
                    // Fallback: sort by createdAt
                    jobs = jobs.OrderByDescending(j => j.CreatedAt).ToList();
                }
            }
            else
            {
                // For anonymous users, sort by createdAt (newest first)
                jobs = jobs.OrderByDescending(j => j.CreatedAt).ToList();
            }
            
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
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Job.NotFound);
                return NotFound(ApiResponse.CreateError(errorMsg, "NotFound"));
            }

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
    /// UC-CAN-15: View AI-Recommended Jobs (Hybrid CF + Semantic)
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

            var recommendations = await _recommendationService.GetRecommendedJobsAsync(userId, limit);

            var result = recommendations.Select(r => new
            {
                job = r.Job,
                hybridScore = Math.Round(r.HybridScore, 2)
            });

            return Ok(ApiResponse<object>.CreateSuccess(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get recommended jobs", "GetRecommendedJobsFailed"));
        }
    }

    /// <summary>
    /// Track user-job interaction (View, Click, Bookmark, Apply) for CF
    /// </summary>
    [HttpPost("{id}/track")]
    [Authorize]
    public async Task<IActionResult> TrackInteraction(string id, [FromBody] TrackInteractionRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _interactionService.TrackInteractionAsync(userId, id, request.Type);
            return Ok(ApiResponse.CreateSuccess("Interaction tracked"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking interaction");
            return StatusCode(500, ApiResponse.CreateError("Failed to track interaction", "TrackInteractionFailed"));
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
            
            // Track bookmark interaction for CF
            try
            {
                await _interactionService.TrackInteractionAsync(userId, id, InteractionType.Bookmark);
            }
            catch (Exception trackEx)
            {
                // Log but don't fail the bookmark operation if tracking fails
                _logger.LogWarning(trackEx, "Failed to track bookmark interaction for CF: {UserId} - {JobId}", userId, id);
            }
            
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
            
            // Note: We don't remove the interaction record when unbookmarking
            // The interaction history is preserved for CF, but future recommendations
            // will naturally reflect the reduced interest over time
            
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
            
            // ? ADD THIS: Trigger immediate job alert matching
            _jobMatchingService.TriggerJobAlertMatching(created.Id);
            
            _logger.LogInformation("Job created successfully: {JobId}. Job alert matching triggered.", created.Id);
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Job.Create);
            return Ok(ApiResponse<Job>.CreateSuccess(created, successMsg));
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
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Job.NotFound);
                return NotFound(ApiResponse.CreateError(errorMsg, "NotFound"));
            }

            if (!string.IsNullOrWhiteSpace(request.Title)) job.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.Description)) job.Description = request.Description;
            if (request.SkillIds != null) job.SkillIds = request.SkillIds;
            if (request.BudgetAmount.HasValue) job.BudgetAmount = request.BudgetAmount;
            if (!string.IsNullOrWhiteSpace(request.Level)) job.Level = request.Level;
            if (!string.IsNullOrWhiteSpace(request.Location)) job.Location = request.Location;
            if (request.Deadline.HasValue) job.Deadline = request.Deadline;

            await _jobService.UpdateAsync(id, job);
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Job.Update);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Job.Update);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.Job.Update);
            return Ok(ApiResponse.CreateSuccess(successMsg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening job");
            return StatusCode(500, ApiResponse.CreateError("Failed to reopen job", "ReopenJobFailed"));
        }
    }

    /// <summary>
    /// Trigger background sync for recommendation skills using Gemini.
    /// Profile page can call this endpoint when user clicks "sync skills for recommended jobs".
    /// </summary>
    [HttpPost("recommended/sync-skills")]
    [Authorize]
    public async Task<IActionResult> SyncRecommendedJobSkills([FromQuery] int limit = 20)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var payload = new RecommendationSyncPayload
            {
                UserId = userId,
                Limit = Math.Clamp(limit, 5, 100)
            };

            var job = new JobMessage
            {
                I18nKey = "Job.Recommendation.SyncSkills",
                Payload = JsonSerializer.Serialize(payload),
                UserId = userId,
                Priority = JobPriority.Normal
            };

            var jobId = await _jobProducer.EnqueueJobAsync(job);
            if (string.IsNullOrWhiteSpace(jobId))
                return StatusCode(500, ApiResponse.CreateError("Failed to enqueue recommendation sync job", "EnqueueFailed"));

            await _progressTracker.InitializeAsync(
                jobId: jobId,
                totalSteps: 4,
                userId: userId,
                jobType: "SyncRecommendationSkills",
                jobTitle: "backgroundJobs.titles.syncSkillsRec");

            await _progressTracker.UpdateProgressAsync(
                jobId: jobId,
                percentComplete: 0,
                currentStep: "backgroundJobs.steps.pending",
                details: new Dictionary<string, object>
                {
                    ["limit"] = payload.Limit
                });

            return Ok(ApiResponse<object>.CreateSuccess(new
            {
                jobId,
                limit = payload.Limit,
                message = "Started syncing skills for recommendations." // this is returned synchronous so it's fine
            }, "Recommendation sync job started"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing recommendation sync job");
            return StatusCode(500, ApiResponse.CreateError("Failed to start recommendation sync", "RecommendationSyncFailed"));
        }
    }
}



