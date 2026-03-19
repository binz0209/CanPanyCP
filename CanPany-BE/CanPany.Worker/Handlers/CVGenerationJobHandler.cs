using CanPany.Application.Interfaces.Services;
using CanPany.Application.Models;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Handlers;

/// <summary>
/// Generates an editable, structured CV (JSON) from candidate profile data using Gemini AI.
/// No Cloudinary upload — JSON is stored in CV.StructuredData; PDF is generated client-side.
/// Handles: Job.CV.Generate.*
/// </summary>
public class CVGenerationJobHandler : BaseJobHandler
{
    private readonly IGeminiService _geminiService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CVGenerationJobHandler> _logger;

    public CVGenerationJobHandler(
        ILogger<CVGenerationJobHandler> logger,
        IGeminiService geminiService,
        IServiceScopeFactory scopeFactory) : base(logger)
    {
        _logger = logger;
        _geminiService = geminiService;
        _scopeFactory = scopeFactory;
    }

    public override string[] SupportedI18nKeys => new[] { "Job.CV.Generate.*" };

    public override async Task<JobResult> ExecuteAsync(
        JobMessage job,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[CV_GEN_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId, job.I18nKey);

        try
        {
            var payload = DeserializePayload<CVGenerationPayload>(job.Payload);

            if (payload == null || string.IsNullOrEmpty(payload.UserId))
                return JobResult.FailureResult("Invalid payload or missing UserId", "INVALID_PAYLOAD");

            using var scope = _scopeFactory.CreateScope();
            var userRepository        = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var userProfileRepository = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();
            var skillRepository       = scope.ServiceProvider.GetRequiredService<ISkillRepository>();
            var cvRepository          = scope.ServiceProvider.GetRequiredService<ICVRepository>();
            var jobRepository         = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            // ── Step 1: Load user ─────────────────────────────────────────────
            await ReportProgressAsync(job.JobId, 10, "Đang tải thông tin ứng viên...", null, cancellationToken);

            var user = await userRepository.GetByIdAsync(payload.UserId);
            if (user == null) return JobResult.FailureResult("User not found", "USER_NOT_FOUND");

            var profile = await userProfileRepository.GetByUserIdAsync(payload.UserId);

            // ── Step 2: Resolve skill names ───────────────────────────────────
            await ReportProgressAsync(job.JobId, 25, "Đang xử lý kỹ năng...", null, cancellationToken);

            var skillNames = new List<string>();
            if (profile?.SkillIds != null && profile.SkillIds.Any())
            {
                foreach (var skillId in profile.SkillIds)
                {
                    try
                    {
                        var skill = await skillRepository.GetByIdAsync(skillId);
                        skillNames.Add(skill?.Name ?? skillId);
                    }
                    catch { skillNames.Add(skillId); }
                }
            }

            // ── Step 3: Build context ─────────────────────────────────────────
            await ReportProgressAsync(job.JobId, 40, "AI đang soạn thảo CV...", null, cancellationToken);

            var ctx = new CVGenerationContext
            {
                FullName       = user.FullName,
                Email          = user.Email,
                Phone          = profile?.Phone,
                Address        = profile?.Address,
                Title          = profile?.Title,
                Bio            = profile?.Bio,
                Experience     = profile?.Experience,
                Education      = profile?.Education,
                Portfolio      = profile?.Portfolio,
                LinkedInUrl    = profile?.LinkedInUrl,
                GitHubUrl      = profile?.GitHubUrl,
                Location       = profile?.Location,
                Skills         = skillNames,
                Languages      = profile?.Languages ?? new List<string>(),
                Certifications = profile?.Certifications ?? new List<string>(),
            };

            // ── Optional: JD-tailored CV ──────────────────────────────────────
            if (!string.IsNullOrEmpty(payload.JobId))
            {
                try
                {
                    var targetJob = await jobRepository.GetByIdAsync(payload.JobId);
                    if (targetJob != null)
                    {
                        ctx.TargetJobTitle       = targetJob.Title;
                        ctx.TargetJobDescription = targetJob.Description;
                        ctx.TargetJobSkillIds    = targetJob.SkillIds ?? new List<string>();
                        _logger.LogInformation("[CV_GEN] Tailoring CV for Job: {Title}", targetJob.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[CV_GEN] Could not load target job {JobId}", payload.JobId);
                }
            }

            // ── Step 4: Generate structured JSON via Gemini ───────────────────
            var cvData = await _geminiService.GenerateCVDataAsync(ctx, cancellationToken);

            await ReportProgressAsync(job.JobId, 80, "Đang lưu CV...", null, cancellationToken);

            // ── Step 5: Save CV entity (no file upload) ───────────────────────
            var dateStr = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var cv = new CV
            {
                UserId        = payload.UserId,
                FileName      = $"AI-CV-{user.FullName.Replace(" ", "-")}-{dateStr}",
                FileUrl       = string.Empty, // No file — editor renders from StructuredData
                FileSize      = 0,
                MimeType      = "application/json",
                IsDefault     = false,
                IsAIGenerated = true,
                StructuredData = cvData,
                CreatedAt     = DateTime.UtcNow,
            };

            var savedCv = await cvRepository.AddAsync(cv);

            await ReportProgressAsync(job.JobId, 100, "Tạo CV thành công!", null, cancellationToken);

            _logger.LogInformation("[CV_GEN_DONE] JobId: {JobId} | CVId: {CVId}", job.JobId, savedCv.Id);

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["CVId"]    = savedCv.Id,
                ["CVName"]  = cv.FileName,
                // No FileUrl — FE navigates to editor using CVId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CV_GEN_FAILED] JobId: {JobId}", job.JobId);
            await ReportProgressAsync(job.JobId, -1, $"Lỗi: {ex.Message}", null, cancellationToken);
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}
