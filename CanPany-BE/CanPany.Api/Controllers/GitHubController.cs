using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
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
    private readonly IUserProfileService _profileService;
    private readonly IGitHubService _gitHubService;
    private readonly II18nService _i18nService;
    private readonly ILogger<GitHubController> _logger;

    public GitHubController(
        IJobProducer jobProducer,
        IJobProgressTracker progressTracker,
        IGitHubAnalysisRepository analysisRepository,
        IUserProfileService profileService,
        IGitHubService gitHubService,
        II18nService i18nService,
        ILogger<GitHubController> logger)
    {
        _jobProducer = jobProducer;
        _progressTracker = progressTracker;
        _analysisRepository = analysisRepository;
        _profileService = profileService;
        _gitHubService = gitHubService;
        _i18nService = i18nService;
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
                    message = _i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError)
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
                message = _i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError)
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
                    message = _i18nService.GetErrorMessage(I18nKeys.Error.BackgroundJob.NotFound),
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
                message = _i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError)
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
                message = _i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError)
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
                message = _i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError)
            });
        }
    }

    /// <summary>
    /// List GitHub repos for current user's linked account.
    /// FE shows this list for user to pick repos to sync.
    /// </summary>
    [HttpGet("repos")]
    [Authorize]
    public async Task<IActionResult> GetLinkedRepos([FromQuery] bool includeForked = false)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.CreateError("User not authenticated", "Unauthorized"));

            var username = await GetLinkedGitHubUsernameAsync(userId);
            if (username == null)
                return BadRequest(ApiResponse.CreateError(
                    "Chưa liên kết GitHub. Vui lòng link GitHub trước.", "GitHubNotLinked"));

            var repos = await _gitHubService.GetUserRepositoriesAsync(username, includeForked);

            var result = repos
                .OrderByDescending(r => r.StarsCount)
                .Select(r => new
                {
                    name = r.Name,
                    fullName = r.FullName,
                    description = r.Description,
                    language = r.Language,
                    stars = r.StarsCount,
                    forks = r.ForksCount,
                    htmlUrl = r.HtmlUrl,
                    isFork = r.IsFork,
                    updatedAt = r.UpdatedAt
                });

            _logger.LogInformation(
                "[GITHUB_REPOS] Found {Count} repos for user {UserId} ({Username})",
                repos.Count, userId, username);

            return Ok(ApiResponse<object>.CreateSuccess(
                new { gitHubUsername = username, totalCount = repos.Count, repositories = result },
                "GitHub repositories loaded"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_REPOS_ERROR]");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "FetchReposFailed"));
        }
    }

    /// <summary>
    /// Sync skills from selected GitHub repos.
    /// Queues a background job to analyze the selected repos with Gemini AI,
    /// then auto-sync extracted skills to UserProfile.
    /// </summary>
    [HttpPost("sync-skills")]
    [Authorize]
    public async Task<IActionResult> SyncSkillsFromSelectedRepos([FromBody] SyncSkillsRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.CreateError("User not authenticated", "Unauthorized"));

            var username = await GetLinkedGitHubUsernameAsync(userId);
            if (username == null)
                return BadRequest(ApiResponse.CreateError(
                    "Chưa liên kết GitHub. Vui lòng link GitHub trước.", "GitHubNotLinked"));

            if (request.RepositoryNames == null || request.RepositoryNames.Count == 0)
                return BadRequest(ApiResponse.CreateError(
                    "Vui lòng chọn ít nhất một repository.", "NoReposSelected"));

            var payload = new GitHubAnalysisPayload
            {
                GitHubUsername = username,
                UserId = userId,
                IncludeForkedRepos = true, // user already picked, don't filter
                AnalyzeSkills = true,
                UpdateProfile = true,
                SelectedRepositories = request.RepositoryNames
            };

            var job = new JobMessage
            {
                I18nKey = "Job.GitHub.Analyze.Profile",
                Payload = JsonSerializer.Serialize(payload),
                UserId = userId,
                Priority = JobPriority.Normal
            };

            var jobId = await _jobProducer.EnqueueJobAsync(job);
            if (string.IsNullOrEmpty(jobId))
                return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "EnqueueFailed"));

            // Initialize progress record immediately so user can track it
            await _progressTracker.InitializeAsync(
                jobId: jobId,
                totalSteps: request.RepositoryNames.Count + 2,
                userId: userId,
                jobType: "SyncSkills",
                jobTitle: "backgroundJobs.titles.syncSkillsGit");

            // Store selected repos context in Details for FE detail view
            await _progressTracker.UpdateProgressAsync(
                jobId: jobId,
                percentComplete: 0,
                currentStep: "backgroundJobs.steps.pending",
                details: new Dictionary<string, object>
                {
                    ["selectedRepos"] = request.RepositoryNames,
                    ["gitHubUsername"] = username,
                    ["count"] = request.RepositoryNames.Count
                });

            _logger.LogInformation(
                "[SYNC_SKILLS] Queued for user {UserId} ({Username}) | Repos: [{Repos}] | JobId: {JobId}",
                userId, username, string.Join(", ", request.RepositoryNames), jobId);

            return Ok(ApiResponse<object>.CreateSuccess(
                new
                {
                    jobId,
                    gitHubUsername = username,
                    selectedRepos = request.RepositoryNames,
                    message = "Analyzing repos and extracting skills..."
                },
                "Skill sync job started"));
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYNC_SKILLS_ERROR]");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "SyncFailed"));
        }
    }

    /// <summary>
    /// Extract GitHub username from user's linked profile
    /// </summary>
    private async Task<string?> GetLinkedGitHubUsernameAsync(string userId)
    {
        var profile = await _profileService.GetByUserIdAsync(userId);
        if (profile == null || string.IsNullOrEmpty(profile.GitHubUrl))
            return null;

        try
        {
            var uri = new Uri(profile.GitHubUrl);
            return uri.AbsolutePath.Trim('/');
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Request model for GitHub analysis
/// </summary>
public record GitHubAnalysisRequest
{
    public string GitHubUsername { get; init; } = null!;
    public bool IncludeForkedRepos { get; init; } = false;
    public bool AnalyzeSkills { get; init; } = true;
    public bool UpdateProfile { get; init; } = true;
}

/// <summary>
/// Request model for selective sync skills
/// </summary>
public record SyncSkillsRequest
{
    /// <summary>
    /// List of repository names (not full_name) to analyze
    /// </summary>
    public List<string> RepositoryNames { get; init; } = new();
}
