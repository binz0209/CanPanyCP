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
            await ThrowIfCancelledAsync(job.JobId, cancellationToken);

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

            // ── Build profile text ──
            var textParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(profile.Title)) textParts.Add($"Role: {profile.Title}");
            if (allSkills.Any()) textParts.Add($"Skills: {string.Join(", ", allSkills.OrderBy(s => s))}");

            var profileText = string.Join(" | ", textParts);
            if (string.IsNullOrWhiteSpace(profileText))
            {
                return JobResult.FailureResult("No profile text/skills available for sync", "NO_PROFILE_DATA");
            }

            await ReportProgressAsync(job.JobId, 70, "backgroundJobs.steps.generatingEmbeddings");
            await ThrowIfCancelledAsync(job.JobId, cancellationToken);

            var hasExistingEmbedding = profile.Embedding != null && profile.Embedding.Count > 0;

            // ── Aggregate skills mới vào profile.SkillIds ──
            var newSkills = allSkills
                .Where(s => profile.SkillIds == null || !profile.SkillIds.Contains(s, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var skillsUpdated = newSkills.Count > 0;
            if (skillsUpdated)
            {
                profile.SkillIds ??= new List<string>();
                profile.SkillIds.AddRange(newSkills);
                // KHÔNG xóa embedding cũ — chỉ thêm skills mới
                // Embedding cũ vẫn hợp lệ cho recommendation
                profile.UpdatedAt = DateTime.UtcNow;

                Logger.LogInformation(
                    "[RECOMMEND_SYNC_SKILLS] JobId: {JobId} | Added {NewCount} new skills. Total: {Total}.",
                    job.JobId, newSkills.Count, profile.SkillIds.Count);
            }

            // ── Tạo embedding NẾU user chưa có (first-time setup) ──
            // Chỉ gọi Gemini 1 lần duy nhất. Sau đó recommendation dùng cached embedding.
            List<double>? embedding = null;
            var embeddingGenerated = false;

            if (!hasExistingEmbedding || payload.ForceRegenerate)
            {
                Logger.LogInformation(
                    "[RECOMMEND_SYNC_EMBED] JobId: {JobId} | {Reason}. Generating embedding via Gemini.",
                    job.JobId, hasExistingEmbedding ? "ForceRegenerate=true" : "No existing embedding");
                try
                {
                    embedding = await _geminiService.GenerateEmbeddingAsync(profileText, cancellationToken);
                    if (embedding != null && embedding.Count > 0)
                    {
                        profile.Embedding = embedding;
                        profile.UpdatedAt = DateTime.UtcNow;
                        embeddingGenerated = true;
                        Logger.LogInformation(
                            "[RECOMMEND_SYNC_EMBED_OK] JobId: {JobId} | Embedding generated ({Size}d) and cached.",
                            job.JobId, embedding.Count);
                    }
                }
                catch (GeminiRateLimitException ex)
                {
                    // Rate limited — skip embedding gracefully, recommendation sẽ dùng skill matching fallback
                    Logger.LogWarning(
                        "[RECOMMEND_SYNC_EMBED_SKIP] JobId: {JobId} | Rate limited (retry after {RetryAfter}s). Embedding skipped — will retry on next sync.",
                        job.JobId, ex.RetryAfterSeconds);
                }
            }
            else
            {
                Logger.LogInformation(
                    "[RECOMMEND_SYNC_EMBED_CACHED] JobId: {JobId} | Existing embedding ({Size}d) kept. Skipping Gemini call.",
                    job.JobId, profile.Embedding!.Count);
                embedding = profile.Embedding;
            }

            // Lưu profile nếu có thay đổi
            if (skillsUpdated || embeddingGenerated)
            {
                await _profileRepository.UpdateAsync(profile);
            }

            await ReportProgressAsync(job.JobId, 100, "backgroundJobs.steps.successSyncRec");

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["UserId"] = payload.UserId,
                ["SkillCount"] = allSkills.Count,
                ["EmbeddingSize"] = profile.Embedding?.Count ?? 0,
                ["CvCount"] = cvs.Count,
                ["HasGitHubAnalysis"] = githubAnalysis != null,
                ["SkillsUpdated"] = skillsUpdated,
                ["NewSkillsAdded"] = newSkills.Count,
                ["EmbeddingGenerated"] = embeddingGenerated,
                ["EmbeddingCached"] = hasExistingEmbedding && !embeddingGenerated
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
