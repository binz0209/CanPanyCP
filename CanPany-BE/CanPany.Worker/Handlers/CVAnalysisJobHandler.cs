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

    public CVAnalysisJobHandler(
        ILogger<CVAnalysisJobHandler> logger,
        IGeminiService geminiService,
        IServiceScopeFactory scopeFactory) : base(logger)
    {
        _logger = logger;
        _geminiService = geminiService;
        _scopeFactory = scopeFactory;
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

            await ReportProgressAsync(job.JobId, 10, "Fetching CV file...");

            // Get CV details
            var cv = await _cvRepository.GetByIdAsync(payload.CVId);
            if (cv == null)
            {
                return JobResult.FailureResult("CV not found", "NOT_FOUND");
            }

            // Read CV physical file
            var relativeUrl = cv.FileUrl;
            if (string.IsNullOrEmpty(relativeUrl))
            {
                return JobResult.FailureResult("CV has no file URL", "NO_FILE");
            }

            // Convert URL back to physical path
            var fileName = relativeUrl.Substring(relativeUrl.LastIndexOf('/') + 1);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cvs", fileName);

            if (!File.Exists(filePath))
            {
                // API uses relative path from CanPany.Api, Worker might run in different directory.
                // Fallback to searching upwards or relative to CanPany.Api
                var solutionDir = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "";
                filePath = Path.Combine(solutionDir, "CanPany.Api", "wwwroot", "cvs", fileName);
                
                if (!File.Exists(filePath))
                {
                   return JobResult.FailureResult($"Physical CV file not found at {filePath}", "FILE_NOT_FOUND");
                }
            }

            await ReportProgressAsync(job.JobId, 30, "Extracting text from PDF...");

            // Extract text from PDF
            var sb = new StringBuilder();
            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
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

            await ReportProgressAsync(job.JobId, 60, "Analyzing CV content with AI...");

            var prompt = $@"
You are an expert technical recruiter and resume parser.
Extract the following information from this CV text:

{cvText}

Provide an objective analysis and return a JSON structured exactly like this:
{{
    ""atsScore"": 85,
    ""scoreBreakdown"": {{
        ""keywords"": 80,
        ""formatting"": 90,
        ""skills"": 85,
        ""experience"": 90,
        ""education"": 80
    }},
    ""extractedSkills"": {{
        ""technical"": [""C#"", "".NET"", ""React"", ""SQL""],
        ""soft"": [""Leadership"", ""Communication""]
    }},
    ""missingKeywords"": [""Docker"", ""Cloud"", ""Microservices""],
    ""improvementSuggestions"": [""Add more metrics to experience"", ""Include links to portfolio""],
    ""summary"": ""A strong mid-level backend developer with good C# skills.""
}}
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

            CVAnalysis? analysisDto = JsonSerializer.Deserialize<CVAnalysis>(cleanJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (analysisDto == null)
            {
                return JobResult.FailureResult("AI response could not be parsed", "DESERIALIZATION_ERROR");
            }

            await ReportProgressAsync(job.JobId, 80, "Saving analysis results...");

            // Update Domain Entity
            analysisDto.CVId = payload.CVId;
            analysisDto.CandidateId = payload.UserId;
            analysisDto.AnalyzedAt = DateTime.UtcNow;

            var savedAnalysis = await _analysisRepository.AddAsync(analysisDto);

            // Update CV record as well
            cv.LatestAnalysisId = savedAnalysis.Id;
            cv.ExtractedSkills = analysisDto.ExtractedSkills.Technical.Concat(analysisDto.ExtractedSkills.Soft).ToList();
            await _cvRepository.UpdateAsync(cv);

            await ReportProgressAsync(job.JobId, 100, "CV analysis completed successfully!");

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["AnalysisId"] = savedAnalysis.Id,
                ["CVId"] = payload.CVId,
                ["ATSScore"] = analysisDto.ATSScore,
                ["PrimarySkills"] = cv.ExtractedSkills,
                ["ImprovementSuggestions"] = analysisDto.ImprovementSuggestions
            });
        }
        catch (Exception _) when (_.GetType().Name.Contains("PdfDocumentFormatException"))
        {
             Logger.LogError(_, "Invalid PDF File Format");
             return JobResult.FailureResult("File could not be parsed as PDF. Unrecognized format.", "PDF_ERROR");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[CV_ANALYSIS_FAILED] JobId: {JobId}", job.JobId);
            await ReportProgressAsync(job.JobId, -1, $"Error: {ex.Message}");
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }
}
