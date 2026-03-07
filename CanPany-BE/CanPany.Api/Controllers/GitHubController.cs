using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Worker.Infrastructure.Progress;
using CanPany.Worker.Infrastructure.Queue;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CanPany.Api.Controllers;

/// <summary>
/// Controller for GitHub integration and analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IJobProducer _jobProducer;
    private readonly IJobProgressTracker _progressTracker;
    private readonly IGitHubAnalysisRepository _analysisRepository;
    private readonly ILogger<GitHubController> _logger;

    public GitHubController(
        IJobProducer jobProducer,
        IJobProgressTracker progressTracker,
        IGitHubAnalysisRepository analysisRepository,
        ILogger<GitHubController> logger)
    {
        _jobProducer = jobProducer;
        _progressTracker = progressTracker;
        _analysisRepository = analysisRepository;
        _logger = logger;
    }

    /// <summary>
    /// Trigger GitHub repository analysis for current user
    /// </summary>
    [HttpPost("analyze")]
    [Authorize]
    public async Task<IActionResult> AnalyzeGitHubProfile([FromBody] GitHubAnalysisRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (string.IsNullOrEmpty(request.GitHubUsername))
            {
                return BadRequest(new { message = "GitHub username is required" });
            }

            // Create job payload
            var payload = new GitHubAnalysisPayload
            {
                GitHubUsername = request.GitHubUsername,
                UserId = userId,
                IncludeForkedRepos = request.IncludeForkedRepos,
                AnalyzeSkills = request.AnalyzeSkills,
                UpdateProfile = request.UpdateProfile
            };

            // Enqueue job
            var job = new JobMessage
            {
                I18nKey = "Job.GitHub.Analyze.Profile",
                Payload = JsonSerializer.Serialize(payload),
                UserId = userId,
                Priority = JobPriority.Normal
            };

            var jobId = await _jobProducer.EnqueueJobAsync(job);

            if (string.IsNullOrEmpty(jobId))
            {
                _logger.LogError(
                    "[GITHUB_ANALYSIS_ENQUEUE_FAILED] UserId: {UserId} | GitHubUsername: {GitHubUsername}",
                    userId,
                    request.GitHubUsername);

                return StatusCode(500, new
                {
                    message = "Failed to enqueue GitHub analysis job"
                });
            }

            _logger.LogInformation(
                "[GITHUB_ANALYSIS_ENQUEUED] JobId: {JobId} | UserId: {UserId} | GitHubUsername: {GitHubUsername}",
                jobId,
                userId,
                request.GitHubUsername);

            return Ok(new
            {
                message = "GitHub analysis job enqueued successfully",
                jobId = jobId,
                gitHubUsername = request.GitHubUsername
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_ANALYSIS_ERROR]");
            return StatusCode(500, new
            {
                message = "An error occurred while processing GitHub analysis request"
            });
        }
    }

    /// <summary>
    /// Get GitHub analysis job status
    /// </summary>
    [HttpGet("status/{jobId}")]
    [Authorize]
    public async Task<IActionResult> GetAnalysisStatus(string jobId)
    {
        try
        {
            var progress = await _progressTracker.GetProgressAsync(jobId);

            if (progress == null)
            {
                return NotFound(new
                {
                    message = "Job not found",
                    jobId = jobId
                });
            }

            return Ok(new
            {
                jobId = progress.JobId,
                status = progress.Status,
                percentComplete = progress.PercentComplete,
                currentStep = progress.CurrentStep,
                startedAt = progress.StartedAt,
                completedAt = progress.CompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET_JOB_STATUS_ERROR] JobId: {JobId}", jobId);
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving job status"
            });
        }
    }

    /// <summary>
    /// Get latest GitHub analysis result for current user
    /// </summary>
    [HttpGet("analysis/latest")]
    [Authorize]
    public async Task<IActionResult> GetLatestAnalysis()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var analysis = await _analysisRepository.GetLatestByUserIdAsync(userId);

            if (analysis == null)
            {
                return NotFound(new
                {
                    message = "No GitHub analysis found for this user"
                });
            }

            return Ok(new
            {
                analysisId = analysis.Id,
                gitHubUsername = analysis.GitHubUsername,
                statistics = new
                {
                    analysis.TotalRepositories,
                    analysis.TotalContributions,
                    analysis.TotalStars,
                    analysis.TotalForks
                },
                languages = analysis.LanguagePercentages
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .Select(kvp => new
                    {
                        language = kvp.Key,
                        percentage = kvp.Value,
                        bytes = analysis.LanguageBytes.GetValueOrDefault(kvp.Key, 0)
                    }),
                skills = new
                {
                    primary = analysis.PrimarySkills,
                    expertiseLevel = analysis.ExpertiseLevel,
                    specializations = analysis.Specializations,
                    proficiency = analysis.SkillAnalysis?.SkillProficiency,
                    recommendations = analysis.SkillAnalysis?.Recommendations
                },
                topRepositories = analysis.TopRepositories.Take(5),
                aiSummary = analysis.AiSummary,
                analyzedAt = analysis.AnalyzedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET_ANALYSIS_ERROR]");
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving analysis"
            });
        }
    }

    /// <summary>
    /// Get analysis history for current user
    /// </summary>
    [HttpGet("analysis/history")]
    [Authorize]
    public async Task<IActionResult> GetAnalysisHistory([FromQuery] int limit = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var analyses = await _analysisRepository.GetByUserIdAsync(userId, limit);

            return Ok(analyses.Select(a => new
            {
                analysisId = a.Id,
                gitHubUsername = a.GitHubUsername,
                totalRepositories = a.TotalRepositories,
                totalStars = a.TotalStars,
                primarySkills = a.PrimarySkills,
                expertiseLevel = a.ExpertiseLevel,
                analyzedAt = a.AnalyzedAt,
                isActive = a.IsActive
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET_HISTORY_ERROR]");
            return StatusCode(500, new
            {
                message = "An error occurred while retrieving analysis history"
            });
        }
    }
}

/// <summary>
/// Request model for GitHub analysis
/// </summary>
public record GitHubAnalysisRequest
{
    /// <summary>
    /// GitHub username to analyze
    /// </summary>
    public string GitHubUsername { get; init; } = null!;

    /// <summary>
    /// Include forked repositories in analysis (default: false)
    /// </summary>
    public bool IncludeForkedRepos { get; init; } = false;

    /// <summary>
    /// Analyze and extract skills using AI (default: true)
    /// </summary>
    public bool AnalyzeSkills { get; init; } = true;

    /// <summary>
    /// Update user profile with analyzed data (default: true)
    /// </summary>
    public bool UpdateProfile { get; init; } = true;
}
