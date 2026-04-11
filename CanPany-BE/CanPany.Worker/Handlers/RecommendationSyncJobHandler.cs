using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Services;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Handlers;

/// <summary>
/// Background job handler to sync recommendation profile context and warm up recommendations.
/// Handles: Job.Recommendation.SyncSkills
/// </summary>
public class RecommendationSyncJobHandler : BaseJobHandler
{
    private readonly IUserProfileRepository _profileRepository;
    private readonly ICVRepository _cvRepository;
    private readonly IGitHubAnalysisRepository _gitHubAnalysisRepository;
    private readonly IGeminiService _geminiService;

    public RecommendationSyncJobHandler(
        ILogger<RecommendationSyncJobHandler> logger,
        IUserProfileRepository profileRepository,
        ICVRepository cvRepository,
        IGitHubAnalysisRepository gitHubAnalysisRepository,
        IGeminiService geminiService) : base(logger)
    {
        _profileRepository = profileRepository;
        _cvRepository = cvRepository;
        _gitHubAnalysisRepository = gitHubAnalysisRepository;
        _geminiService = geminiService;
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.Recommendation.SyncSkills"
    };

    public override async Task<JobResult> ExecuteAsync(
        JobMessage job,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("[RECOMMEND_SYNC_START] JobId: {JobId}", job.JobId);

        try
        {
            var payload = DeserializePayload<RecommendationSyncPayload>(job.Payload);
            if (payload == null || string.IsNullOrWhiteSpace(payload.UserId))
            {
                return JobResult.FailureResult("Invalid payload or missing UserId", "INVALID_PAYLOAD");
            }

            await ReportProgressAsync(job.JobId, 15, "backgroundJobs.steps.loadingProfile");

            var profile = await _profileRepository.GetByUserIdAsync(payload.UserId);
            if (profile == null)
            {
                return JobResult.FailureResult("User profile not found", "PROFILE_NOT_FOUND");
            }

            await ReportProgressAsync(job.JobId, 40, "backgroundJobs.steps.extractingSkills");

            var allSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (profile.SkillIds != null)
            {
                foreach (var skill in profile.SkillIds)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                        allSkills.Add(skill);
                }
            }

            var cvs = (await _cvRepository.GetByUserIdAsync(payload.UserId)).ToList();
            foreach (var cv in cvs)
            {
                if (cv.ExtractedSkills == null) continue;
                foreach (var skill in cv.ExtractedSkills)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                        allSkills.Add(skill);
                }
            }

            var githubAnalysis = await _gitHubAnalysisRepository.GetLatestByUserIdAsync(payload.UserId);
            if (githubAnalysis?.PrimarySkills != null)
            {
                foreach (var skill in githubAnalysis.PrimarySkills)
                {
                    if (!string.IsNullOrWhiteSpace(skill))
                        allSkills.Add(skill);
                }
            }

            if (githubAnalysis?.LanguagePercentages != null)
            {
                foreach (var language in githubAnalysis.LanguagePercentages.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(language))
                        allSkills.Add(language);
                }
            }

            var textParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(profile.Title)) textParts.Add(profile.Title);
            if (!string.IsNullOrWhiteSpace(profile.Bio)) textParts.Add(profile.Bio);
            if (!string.IsNullOrWhiteSpace(profile.Experience)) textParts.Add(profile.Experience);
            if (!string.IsNullOrWhiteSpace(profile.Education)) textParts.Add(profile.Education);
            if (allSkills.Any()) textParts.Add(string.Join(" ", allSkills));

            var profileText = string.Join(" ", textParts);
            if (string.IsNullOrWhiteSpace(profileText))
            {
                return JobResult.FailureResult("No profile text/skills available for sync", "NO_PROFILE_DATA");
            }

            await ReportProgressAsync(job.JobId, 70, "backgroundJobs.steps.generatingEmbeddings");

            var embedding = await _geminiService.GenerateEmbeddingAsync(profileText);

            profile.Embedding = embedding;
            profile.UpdatedAt = DateTime.UtcNow;
            await _profileRepository.UpdateAsync(profile);

            await ReportProgressAsync(job.JobId, 100, "backgroundJobs.steps.successSyncRec");

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["UserId"] = payload.UserId,
                ["SkillCount"] = allSkills.Count,
                ["EmbeddingSize"] = embedding.Count,
                ["CvCount"] = cvs.Count,
                ["HasGitHubAnalysis"] = githubAnalysis != null
            });
        }
        catch (GeminiRateLimitException ex)
        {
            Logger.LogWarning(ex,
                "[RECOMMEND_SYNC_RATE_LIMITED] JobId: {JobId} | RetryAfter: {RetryAfter}s",
                job.JobId, ex.RetryAfterSeconds);

            await ReportProgressAsync(job.JobId, -1,
                $"Gemini đang rate limit. Sẽ retry sau {ex.RetryAfterSeconds}s...");

            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[RECOMMEND_SYNC_FAILED] JobId: {JobId}", job.JobId);
            await ReportProgressAsync(job.JobId, -1, "backgroundJobs.steps.error", new Dictionary<string, object> { ["message"] = ex.Message });
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}
