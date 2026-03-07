using CanPany.Application.Interfaces.Services;
using CanPany.Domain.DTOs.Analysis;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Worker.Models;
using CanPany.Worker.Models.Payloads;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CanPany.Worker.Handlers;

/// <summary>
/// Job handler for analyzing GitHub repositories and extracting user skills
/// Handles: Job.GitHub.Analyze.*
/// </summary>
public class GitHubAnalysisJobHandler : BaseJobHandler
{
    private readonly IGitHubService _gitHubService;
    private readonly IGeminiService _geminiService;
    private readonly IGitHubAnalysisRepository _analysisRepository;

    public GitHubAnalysisJobHandler(
        ILogger<GitHubAnalysisJobHandler> logger,
        IGitHubService gitHubService,
        IGeminiService geminiService,
        IGitHubAnalysisRepository analysisRepository) : base(logger)
    {
        _gitHubService = gitHubService;
        _geminiService = geminiService;
        _analysisRepository = analysisRepository;
    }

    public override string[] SupportedI18nKeys => new[]
    {
        "Job.GitHub.Analyze.*",
        "Job.GitHub.ExtractSkills.*"
    };

    public override async Task<JobResult> ExecuteAsync(
        JobMessage job,
        CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(
            "[GITHUB_ANALYSIS_START] JobId: {JobId} | I18nKey: {I18nKey}",
            job.JobId,
            job.I18nKey
        );

        try
        {
            var payload = DeserializePayload<GitHubAnalysisPayload>(job.Payload);

            if (payload == null || string.IsNullOrEmpty(payload.GitHubUsername))
            {
                return JobResult.FailureResult(
                    "Invalid payload or missing GitHub username",
                    "INVALID_PAYLOAD");
            }

            await ReportProgressAsync(job.JobId, 10, "Fetching GitHub repositories...");

            // Step 1: Fetch GitHub data
            var contributionSummary = await _gitHubService.GetUserContributionSummaryAsync(
                payload.GitHubUsername,
                payload.IncludeForkedRepos,
                cancellationToken);

            if (contributionSummary.TotalRepositories == 0)
            {
                Logger.LogWarning(
                    "[GITHUB_NO_REPOS] No repositories found for user: {Username}",
                    payload.GitHubUsername);

                return JobResult.FailureResult(
                    "No repositories found for this GitHub user",
                    "NO_REPOSITORIES");
            }

            await ReportProgressAsync(job.JobId, 40, 
                $"Found {contributionSummary.TotalRepositories} repositories. Analyzing languages...");

            // Step 2: Prepare data summary
            var languagePercentages = contributionSummary.GetLanguagePercentages();
            var topLanguages = contributionSummary.GetTopLanguages(10);

            Logger.LogInformation(
                "[GITHUB_STATS] User: {Username} | Repos: {Repos} | Languages: {Languages} | Stars: {Stars} | Contributions: {Contributions}",
                payload.GitHubUsername,
                contributionSummary.TotalRepositories,
                string.Join(", ", topLanguages),
                contributionSummary.TotalStars,
                contributionSummary.TotalContributions
            );

            await ReportProgressAsync(job.JobId, 60, "Preparing data for skill analysis...");

            // Step 3: Analyze skills with Gemini (if requested)
            SkillAnalysisDto? skillAnalysisDto = null;
            if (payload.AnalyzeSkills)
            {
                await ReportProgressAsync(job.JobId, 70, "Analyzing skills with AI...");

                // Use Gemini service to analyze skills
                skillAnalysisDto = await _geminiService.AnalyzeGitHubSkillsAsync(
                    payload.GitHubUsername,
                    languagePercentages,
                    contributionSummary.TotalRepositories,
                    contributionSummary.TotalStars,
                    contributionSummary.TotalContributions,
                    cancellationToken);

                Logger.LogInformation(
                    "[GEMINI_ANALYSIS] Generated skill analysis for {Username}. Skills: {Skills}",
                    payload.GitHubUsername,
                    skillAnalysisDto != null ? string.Join(", ", skillAnalysisDto.PrimarySkills) : "None");
            }

            await ReportProgressAsync(job.JobId, 80, "Saving analysis to RAG storage...");

            // Step 4: Save to RAG Storage (GitHubAnalysisResult)
            var analysisResult = new GitHubAnalysisResult
            {
                UserId = payload.UserId,
                GitHubUsername = payload.GitHubUsername,
                JobId = job.JobId,
                TotalRepositories = contributionSummary.TotalRepositories,
                TotalContributions = contributionSummary.TotalContributions,
                TotalStars = contributionSummary.TotalStars,
                TotalForks = contributionSummary.TotalForks,
                LanguageBytes = contributionSummary.LanguageBytes,
                LanguagePercentages = languagePercentages,
                TopRepositories = contributionSummary.Repositories
                    .OrderByDescending(r => r.StarsCount)
                    .Take(10)
                    .Select(r => new GitHubRepositorySummary
                    {
                        Name = r.Name,
                        Description = r.Description,
                        Language = r.Language,
                        StarsCount = r.StarsCount,
                        ForksCount = r.ForksCount,
                        HtmlUrl = r.HtmlUrl,
                        IsFork = r.IsFork
                    })
                    .ToList(),
                SkillAnalysis = skillAnalysisDto,
                PrimarySkills = skillAnalysisDto?.PrimarySkills ?? new List<string>(),
                ExpertiseLevel = skillAnalysisDto?.ExpertiseLevel,
                Specializations = skillAnalysisDto?.Specializations ?? new List<string>(),
                AiSummary = skillAnalysisDto?.Summary,
                AnalyzedAt = DateTime.UtcNow,
                IncludeForkedRepos = payload.IncludeForkedRepos,
                IsActive = true,
                RawData = JsonSerializer.Serialize(contributionSummary)
            };

            // Save to database
            var savedAnalysis = await _analysisRepository.AddAsync(analysisResult);

            // Set this as the active analysis (deactivate old ones)
            await _analysisRepository.SetActiveAnalysisAsync(payload.UserId, savedAnalysis.Id);

            Logger.LogInformation(
                "[RAG_STORAGE] Saved analysis: {AnalysisId} for user: {UserId}",
                savedAnalysis.Id,
                payload.UserId);

            await ReportProgressAsync(job.JobId, 90, "Finalizing analysis results...");

            // Step 5: Build result
            var result = new
            {
                AnalysisId = savedAnalysis.Id,
                GitHubUsername = payload.GitHubUsername,
                UserId = payload.UserId,
                Statistics = new
                {
                    contributionSummary.TotalRepositories,
                    contributionSummary.TotalContributedRepositories,
                    contributionSummary.TotalStars,
                    contributionSummary.TotalForks,
                    contributionSummary.TotalContributions
                },
                Languages = languagePercentages.Select(kvp => new
                {
                    Language = kvp.Key,
                    Percentage = kvp.Value,
                    Bytes = contributionSummary.LanguageBytes[kvp.Key]
                }).ToList(),
                TopRepositories = savedAnalysis.TopRepositories,
                SkillAnalysis = skillAnalysisDto,
                AnalyzedAt = savedAnalysis.AnalyzedAt
            };

            await ReportProgressAsync(job.JobId, 100, "GitHub analysis completed successfully!");

            Logger.LogInformation(
                "[GITHUB_ANALYSIS_SUCCESS] JobId: {JobId} | AnalysisId: {AnalysisId} | User: {Username} | Repos: {Repos} | Skills: {Skills}",
                job.JobId,
                savedAnalysis.Id,
                payload.GitHubUsername,
                contributionSummary.TotalRepositories,
                string.Join(", ", savedAnalysis.PrimarySkills)
            );

            return JobResult.SuccessResult(new Dictionary<string, object?>
            {
                ["AnalysisId"] = savedAnalysis.Id,
                ["GitHubUsername"] = payload.GitHubUsername,
                ["UserId"] = payload.UserId,
                ["TotalRepositories"] = contributionSummary.TotalRepositories,
                ["TopLanguages"] = topLanguages,
                ["PrimarySkills"] = savedAnalysis.PrimarySkills,
                ["ExpertiseLevel"] = savedAnalysis.ExpertiseLevel,
                ["SkillAnalysis"] = skillAnalysisDto,
                ["FullAnalysis"] = result
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[GITHUB_ANALYSIS_FAILED] JobId: {JobId}", job.JobId);
            await ReportProgressAsync(job.JobId, -1, $"Error: {ex.Message}");
            return JobResult.FailureResult(ex.Message, ex.GetType().Name);
        }
    }

    /// <summary>
    /// Build prompt for Gemini to analyze developer skills from GitHub data
    /// </summary>
    private string BuildSkillAnalysisPrompt(
        Domain.DTOs.GitHub.GitHubUserContributionSummary summary,
        Dictionary<string, double> languagePercentages)
    {
        var languageBreakdown = string.Join("\n", 
            languagePercentages
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .Select(kvp => $"- {kvp.Key}: {kvp.Value}%"));

        var topRepos = string.Join("\n",
            summary.Repositories
                .OrderByDescending(r => r.StarsCount)
                .Take(10)
                .Select(r => $"- {r.Name} ({r.Language}): {r.StarsCount} ⭐ - {r.Description ?? "No description"}"));

        return $@"
Analyze this GitHub profile and extract technical skills, expertise level, and recommendations:

**GitHub User Statistics:**
- Username: {summary.Username}
- Total Repositories: {summary.TotalRepositories}
- Total Contributions: {summary.TotalContributions}
- Total Stars Received: {summary.TotalStars}
- Total Forks: {summary.TotalForks}

**Programming Languages (by usage %):**
{languageBreakdown}

**Top Repositories:**
{topRepos}

**Analysis Request:**
Based on the above data, please provide:

1. **Primary Skills**: List the top 5-7 technical skills this developer has based on their language usage and projects
2. **Expertise Level**: Rate their overall expertise (Junior/Mid/Senior/Expert) with justification
3. **Specializations**: Identify any specializations (e.g., Web Development, Mobile, DevOps, Data Science)
4. **Skill Proficiency**: For each primary skill, estimate proficiency level (Beginner/Intermediate/Advanced/Expert)
5. **Recommendations**: Suggest 2-3 skills they could learn to complement their current skillset

Please format the response as JSON with this structure:
{{
  ""primarySkills"": [""skill1"", ""skill2"", ...],
  ""expertiseLevel"": ""Mid/Senior/etc"",
  ""specializations"": [""specialization1"", ...],
  ""skillProficiency"": {{""skill"": ""level"", ...}},
  ""recommendations"": [""skill1"", ""skill2"", ...],
  ""summary"": ""Brief 2-3 sentence summary of the developer's profile""
}}
";
    }
}
