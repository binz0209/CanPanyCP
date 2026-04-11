using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using System.Text.Json;
using System.Text;

namespace CanPany.Worker.Handlers;

/// <summary>
/// Job handler for analyzing PDF CVs and extracting candidate skills
/// Handles: Job.CV.Analyze.*
/// </summary>
public class CVAnalysisJobHandler : BaseJobHandler
{
    private readonly IGeminiService _geminiService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CVAnalysisJobHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CVAnalysisJobHandler(
        ILogger<CVAnalysisJobHandler> logger,
        IGeminiService geminiService,
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory) : base(logger)
    {
        _logger = logger;
        _geminiService = geminiService;
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.CV.Analyze.*"
    };

    public override async Task<JobResult> ExecuteAsync(
        JobMessage job,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "[CV_ANALYSIS_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId,
            job.I18nKey
        );

        try
        {
            var payload = DeserializePayload<CVAnalysisPayload>(job.Payload);

            if (payload == null || string.IsNullOrEmpty(payload.CVId))
            {
                return JobResult.FailureResult(
                    "Invalid payload or missing CV ID",
                    "INVALID_PAYLOAD");
            }

            using var scope = _scopeFactory.CreateScope();
            var _cvRepository = scope.ServiceProvider.GetRequiredService<ICVRepository>();
            var _analysisRepository = scope.ServiceProvider.GetRequiredService<ICVAnalysisRepository>();
            var _userProfileRepository = scope.ServiceProvider.GetRequiredService<IUserProfileRepository>();

            await ReportProgressAsync(job.JobId, 10, "backgroundJobs.steps.fetchingCv");

            // Get CV details
            var cv = await _cvRepository.GetByIdAsync(payload.CVId);
            if (cv == null)
            {
                return JobResult.FailureResult("CV not found", "NOT_FOUND");
            }

            if (string.IsNullOrEmpty(cv.FileUrl))
            {
                return JobResult.FailureResult("CV has no file URL", "NO_FILE");
            }

            // Download CV file from Cloudinary URL (not local disk)
            byte[] fileBytes;
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                fileBytes = await httpClient.GetByteArrayAsync(cv.FileUrl, cancellationToken);
                _logger.LogInformation("[CV_ANALYSIS] Downloaded CV from URL: {Url}, Size: {Size} bytes", cv.FileUrl, fileBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CV_ANALYSIS] Failed to download CV from URL: {Url}", cv.FileUrl);
                return JobResult.FailureResult($"Failed to download CV file: {ex.Message}", "DOWNLOAD_FAILED");
            }

            await ReportProgressAsync(job.JobId, 30, "backgroundJobs.steps.extractingCvText");

            // Extract text from PDF bytes
            var sb = new StringBuilder();
            try
            {
                using var stream = new MemoryStream(fileBytes);
                using var document = PdfDocument.Open(stream);
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CV_ANALYSIS] Failed to parse PDF");
                return JobResult.FailureResult("File could not be parsed as PDF. Unrecognized format.", "PDF_ERROR");
            }

            var cvText = sb.ToString();

            if (string.IsNullOrWhiteSpace(cvText))
            {
                return JobResult.FailureResult("CV file contains no readable text", "EMPTY_PDF");
            }

            // Truncate text if it's too long
            if (cvText.Length > 20000)
            {
                cvText = cvText.Substring(0, 20000);
            }

            await ReportProgressAsync(job.JobId, 60, "backgroundJobs.steps.analyzingCvAi");

            // New comprehensive prompt that extracts profile data for updating user profile
            var prompt = $@"
You are an expert technical recruiter and resume parser.
Analyze this CV text and extract ALL profile information.

{cvText}

Provide a comprehensive analysis with EXACTLY this JSON structure (no extra keys, no markdown):
{{
    ""profile"": {{
        ""fullName"": ""John Doe"",
        ""email"": ""john@example.com"",
        ""phone"": ""+84 123 456 789"",
        ""title"": ""Senior Software Engineer"",
        ""bio"": ""Experienced developer with 5+ years in..."",
        ""location"": ""Ho Chi Minh City, Vietnam"",
        ""address"": ""123 Main St, District 1, HCMC"",
        ""linkedInUrl"": ""https://linkedin.com/in/johndoe"",
        ""gitHubUrl"": ""https://github.com/johndoe"",
        ""portfolio"": ""https://johndoe.dev""
    }},
    ""experience"": ""- Senior Developer at ABC Corp (2020-Present): Led team of 5 developers...\n- Developer at XYZ Inc (2018-2020): Built REST APIs using Node.js..."",
    ""education"": ""- Bachelor of Computer Science, ABC University (2014-2018)\n- High School, XYZ High School (2011-2014)"",
    ""extractedSkills"": {{
        ""technical"": [""C#"", "".NET"", ""React"", ""SQL Server"", ""Azure"", ""Docker""],
        ""soft"": [""Leadership"", ""Communication"", ""Problem Solving""]
    }},
    ""languages"": [""English (Fluent)"", ""Vietnamese (Native)""],
    ""certifications"": [""AWS Solutions Architect"", ""Microsoft Azure Developer Associate""],
    ""atsScore"": 85,
    ""scoreBreakdown"": {{
        ""keywords"": 80,
        ""formatting"": 90,
        ""skills"": 85,
        ""experience"": 90,
        ""education"": 80
    }},
    ""missingKeywords"": [""Docker"", ""Cloud"", ""Microservices""],
    ""improvementSuggestions"": [""Add more metrics to experience"", ""Include links to portfolio""],
    ""summary"": ""A strong mid-level backend developer with good C# skills.""
}}

IMPORTANT:
- Extract ALL information available in the CV
- If a field is not found, use empty string """" or empty array []
- For experience and education, provide as detailed text as possible
- Return ONLY valid JSON, no explanation
";

            var systemPrompt = "You are a Resume Parser API. Return ONLY raw JSON matching the requested structure.";

            var geminiResponse = await _geminiService.GenerateChatResponseAsync(systemPrompt, prompt);

            // Clean up Gemini markdown fences if present
            var jsonStart = geminiResponse.IndexOf('{');
            var jsonEnd = geminiResponse.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return JobResult.FailureResult("AI did not return valid JSON", "PARSE_ERROR");
            }

            var cleanJson = geminiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);

            // Parse the comprehensive CV extraction response
            CVExtractedProfile? extractedProfile = JsonSerializer.Deserialize<CVExtractedProfile>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (extractedProfile == null)
            {
                return JobResult.FailureResult("AI response could not be parsed", "DESERIALIZATION_ERROR");
            }

            // Convert to CVAnalysis format for storage
            var analysisDto = new CVAnalysis
            {
                CVId = payload.CVId,
                CandidateId = payload.UserId,
                ATSScore = extractedProfile.AtsScore ?? 0,
                MissingKeywords = extractedProfile.MissingKeywords ?? new List<string>(),
                ImprovementSuggestions = extractedProfile.ImprovementSuggestions ?? new List<string>(),
                ExtractedSkills = new Domain.Entities.ExtractedSkills
                {
                    Technical = extractedProfile.ExtractedSkills?.Technical ?? new List<string>(),
                    Soft = extractedProfile.ExtractedSkills?.Soft ?? new List<string>()
                },
                ScoreBreakdown = extractedProfile.ScoreBreakdown != null ? new ATSScoreBreakdown
                {
                    Keywords = extractedProfile.ScoreBreakdown.Keywords,
                    Formatting = extractedProfile.ScoreBreakdown.Formatting,
                    Skills = extractedProfile.ScoreBreakdown.Skills,
                    Experience = extractedProfile.ScoreBreakdown.Experience,
                    Education = extractedProfile.ScoreBreakdown.Education
                } : new ATSScoreBreakdown(),
                // Map profile extraction data
                Profile = extractedProfile.Profile != null ? new Domain.Entities.ExtractedProfile
                {
                    FullName = extractedProfile.Profile.FullName,
                    Email = extractedProfile.Profile.Email,
                    Phone = extractedProfile.Profile.Phone,
                    Title = extractedProfile.Profile.Title,
                    Location = extractedProfile.Profile.Location,
                    Address = extractedProfile.Profile.Address,
                    LinkedIn = extractedProfile.Profile.LinkedInUrl,
                    GitHub = extractedProfile.Profile.GitHubUrl,
                    Portfolio = extractedProfile.Profile.Portfolio,
                    Summary = extractedProfile.Profile.Bio
                } : null,
                Experience = extractedProfile.Experience,
                Education = extractedProfile.Education,
                Languages = extractedProfile.Languages?.Select(l => new Domain.Entities.ExtractedLanguage { Language = l, Level = "" }).ToList() ?? new List<Domain.Entities.ExtractedLanguage>(),
                Certifications = extractedProfile.Certifications?.Select(c => new Domain.Entities.ExtractedCertification { Name = c }).ToList() ?? new List<Domain.Entities.ExtractedCertification>()
            };

            await ReportProgressAsync(job.JobId, 80, "backgroundJobs.steps.savingCvResults");

            // Update Domain Entity
            analysisDto.CVId = payload.CVId;
            analysisDto.CandidateId = payload.UserId;
            analysisDto.AnalyzedAt = DateTime.UtcNow;

            var savedAnalysis = await _analysisRepository.AddAsync(analysisDto);

            // Update CV record as well
            cv.LatestAnalysisId = savedAnalysis.Id;
            var technical = analysisDto.ExtractedSkills?.Technical ?? new List<string>();
            var soft = analysisDto.ExtractedSkills?.Soft ?? new List<string>();
            var combinedSkills = technical.Concat(soft)
                                          .Where(s => !string.IsNullOrWhiteSpace(s))
                                          .ToList();

            Logger.LogInformation("[CV_ANALYSIS] Extracted {Count} skills: {Skills}", combinedSkills.Count, string.Join(", ", combinedSkills));

            cv.ExtractedSkills = combinedSkills;
            await _cvRepository.UpdateAsync(cv);

            // Sync with UserProfile - Update ALL extracted profile data
            var userProfile = await _userProfileRepository.GetByUserIdAsync(payload.UserId);
            if (userProfile == null)
            {
                // Create new profile if doesn't exist
                userProfile = new UserProfile
                {
                    UserId = payload.UserId
                };
            }

            // Update skills - add new skills without duplicates
            var existingSkills = userProfile.SkillIds.Select(s => s.ToLower()).ToHashSet();
            foreach (var skill in combinedSkills)
            {
                if (!existingSkills.Contains(skill.ToLower()))
                {
                    userProfile.SkillIds.Add(skill);
                    existingSkills.Add(skill.ToLower());
                }
            }

            // Update profile with extracted data (only if not already set or if CV analysis provides better data)
            if (analysisDto.Profile != null)
            {
                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Summary))
                    userProfile.Bio = analysisDto.Profile.Summary;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Phone))
                    userProfile.Phone = analysisDto.Profile.Phone;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Title))
                    userProfile.Title = analysisDto.Profile.Title;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Location))
                    userProfile.Location = analysisDto.Profile.Location;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Address))
                    userProfile.Address = analysisDto.Profile.Address;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.LinkedIn))
                    userProfile.LinkedInUrl = analysisDto.Profile.LinkedIn;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.GitHub))
                    userProfile.GitHubUrl = analysisDto.Profile.GitHub;

                if (!string.IsNullOrWhiteSpace(analysisDto.Profile.Portfolio))
                    userProfile.Portfolio = analysisDto.Profile.Portfolio;
            }

            // Update experience if extracted
            if (!string.IsNullOrWhiteSpace(analysisDto.Experience))
            {
                userProfile.Experience = analysisDto.Experience;
                Logger.LogInformation("[CV_ANALYSIS] Extracted experience: {Experience}",
                    analysisDto.Experience.Substring(0, Math.Min(100, analysisDto.Experience.Length)));
            }

            // Update education if extracted
            if (!string.IsNullOrWhiteSpace(analysisDto.Education))
            {
                userProfile.Education = analysisDto.Education;
                Logger.LogInformation("[CV_ANALYSIS] Extracted education: {Education}",
                    analysisDto.Education.Substring(0, Math.Min(100, analysisDto.Education.Length)));
            }

            // Update languages if extracted
            if (analysisDto.Languages != null && analysisDto.Languages.Count > 0)
            {
                var existingLangs = userProfile.Languages.Select(l => l.ToLower()).ToHashSet();
                foreach (var lang in analysisDto.Languages)
                {
                    var langName = lang.Language ?? "";
                    if (!string.IsNullOrWhiteSpace(langName) && !existingLangs.Contains(langName.ToLower()))
                    {
                        userProfile.Languages.Add(langName);
                        existingLangs.Add(langName.ToLower());
                    }
                }
                Logger.LogInformation("[CV_ANALYSIS] Extracted {Count} languages: {Languages}",
                    analysisDto.Languages.Count, string.Join(", ", analysisDto.Languages.Select(l => l.Language)));
            }

            // Update certifications if extracted
            if (analysisDto.Certifications != null && analysisDto.Certifications.Count > 0)
            {
                var existingCerts = userProfile.Certifications.Select(c => c.ToLower()).ToHashSet();
                foreach (var cert in analysisDto.Certifications)
                {
                    var certName = cert.Name ?? "";
                    if (!string.IsNullOrWhiteSpace(certName) && !existingCerts.Contains(certName.ToLower()))
                    {
                        userProfile.Certifications.Add(certName);
                        existingCerts.Add(certName.ToLower());
                    }
                }
                Logger.LogInformation("[CV_ANALYSIS] Extracted {Count} certifications: {Certifications}",
                    analysisDto.Certifications.Count, string.Join(", ", analysisDto.Certifications.Select(c => c.Name)));
            }

            userProfile.MarkAsUpdated();
            await _userProfileRepository.UpdateAsync(userProfile);

            Logger.LogInformation("[CV_ANALYSIS_PROFILE_UPDATED] UserId: {UserId} - Profile updated with CV data", payload.UserId);

            await ReportProgressAsync(job.JobId, 100, "backgroundJobs.steps.successCvAnalyze");

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["AnalysisId"] = savedAnalysis.Id,
                ["CVId"] = payload.CVId,
                ["ATSScore"] = analysisDto.ATSScore,
                ["PrimarySkills"] = cv.ExtractedSkills,
                ["ImprovementSuggestions"] = analysisDto.ImprovementSuggestions
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CV_ANALYSIS_FAILED] JobId: {JobId}", job.JobId);
            await ReportProgressAsync(job.JobId, -1, "backgroundJobs.steps.error", new Dictionary<string, object> { ["message"] = ex.Message });
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}
