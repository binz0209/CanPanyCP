using System.Net;
using System.Text;
using System.Text.Json;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.DTOs.Analysis;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// Custom exception for Gemini 429 rate limiting
/// </summary>
public class GeminiRateLimitException : Exception
{
    public int RetryAfterSeconds { get; }

    public GeminiRateLimitException(int retryAfterSeconds)
        : base($"Gemini API rate limited. Retry after {retryAfterSeconds}s")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public GeminiRateLimitException(int retryAfterSeconds, Exception innerException)
        : base($"Gemini API rate limited. Retry after {retryAfterSeconds}s", innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Gemini AI Service - Using Gemini 2.0 Flash Experimental
/// - Chat: gemini-2.0-flash-exp (latest experimental model)
/// - Embedding: text-embedding-004 (latest embedding model)
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _chatModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";

    // Fallback order for embedding models/endpoints
    // gemini-embedding-001 is the current working model (text-embedding-004 & embedding-001 deprecated)
    // outputDimensionality is set to 768 to match existing MongoDB vector_index and stored embeddings
    private const int EmbeddingDimensions = 768;
    private static readonly (string Model, string Url)[] EmbeddingCandidates =
    {
        ("models/gemini-embedding-001", "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent"),
        ("models/text-embedding-004", "https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent"),
        ("models/embedding-001", "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent")
    };

    // Rate limit config
    private const int MaxRetries = 3;
    private const int BaseRetryDelaySeconds = 10;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["GoogleGemini:ApiKey"] ?? string.Empty;

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogInformation(
                "[GEMINI_INIT] Using Gemini 2.0 Flash Experimental | Embedding: text-embedding-004");
        }
    }

    /// <summary>
    /// Send HTTP POST with 429 retry + exponential backoff.
    /// Reads Retry-After header when available.
    /// </summary>
    private async Task<HttpResponseMessage> SendWithRetryAsync(
        string url,
        StringContent content,
        CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            // Clone content for retry (StringContent can only be read once)
            var clonedContent = new StringContent(
                await content.ReadAsStringAsync(cancellationToken),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, clonedContent, cancellationToken);

            // Retry on 429 (rate limit) and 503 (transient overload)
            if (response.StatusCode != HttpStatusCode.TooManyRequests &&
                response.StatusCode != HttpStatusCode.ServiceUnavailable)
                return response;

            if (attempt == MaxRetries)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    double finalWait = response.Headers.RetryAfter?.Delta?.TotalSeconds
                        ?? Math.Pow(2, attempt) * BaseRetryDelaySeconds;
                    throw new GeminiRateLimitException((int)finalWait);
                }
                // 503 exhausted retries — let it propagate as HttpRequestException
                return response;
            }

            // Parse Retry-After header (seconds) or use exponential backoff
            double retryAfterSeconds;
            if (response.StatusCode == HttpStatusCode.TooManyRequests &&
                response.Headers.RetryAfter?.Delta != null)
            {
                retryAfterSeconds = response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            }
            else
            {
                retryAfterSeconds = Math.Pow(2, attempt) * BaseRetryDelaySeconds;
            }

            _logger.LogWarning(
                "[GEMINI_RETRY] Status: {Status}. Attempt {Attempt}/{MaxRetries}. Waiting {Seconds}s before retry",
                response.StatusCode, attempt + 1, MaxRetries, retryAfterSeconds);

            await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds), cancellationToken);
        }

        // Unreachable, but compiler needs it
        throw new GeminiRateLimitException(60);
    }

    public async Task<List<double>> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("[GEMINI_EMBED] API Key missing. Returning mock embedding.");
            return GenerateMockEmbedding();
        }

        try
        {
            for (int i = 0; i < EmbeddingCandidates.Length; i++)
            {
                var (model, url) = EmbeddingCandidates[i];

                var requestBody = new
                {
                    model,
                    content = new { parts = new[] { new { text } } },
                    outputDimensionality = EmbeddingDimensions
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await SendWithRetryAsync($"{url}?key={_apiKey}", content);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(
                        "[GEMINI_EMBED_404] Embedding endpoint/model not found. Trying fallback {Index}/{Total}. Model: {Model}",
                        i + 1,
                        EmbeddingCandidates.Length,
                        model);
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Gemini Embedding API Error: {response.StatusCode} - {errorBody}",
                        null,
                        response.StatusCode);
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);

                var values = doc.RootElement
                    .GetProperty("embedding")
                    .GetProperty("values");

                var result = new List<double>();
                foreach (var value in values.EnumerateArray())
                {
                    result.Add(value.GetDouble());
                }

                return result;
            }

            throw new HttpRequestException("Gemini embedding failed: all configured embedding endpoints returned 404.", null, HttpStatusCode.NotFound);
        }
        catch (GeminiRateLimitException)
        {
            // Re-throw so Worker Polly can retry the job
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GEMINI_EMBED_ERROR] Error generating embedding");
            // Throw instead of silent mock fallback — let caller decide
            throw;
        }
    }

    public async Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Gemini API Key is missing. Please configure it in appsettings.json.");
        }

        try
        {
            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("[GEMINI_REQUEST] URL: {Url}", _chatModelUrl);

            // Use retry-aware send
            var response = await SendWithRetryAsync($"{_chatModelUrl}?key={_apiKey}", content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[GEMINI_ERROR] Status: {Status} | Response: {Response}",
                    response.StatusCode, responseString);
                throw new HttpRequestException(
                    $"Gemini API Error: {response.StatusCode} - {responseString}",
                    null,
                    response.StatusCode);
            }

            using var doc = JsonDocument.Parse(responseString);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            _logger.LogDebug("[GEMINI_RESPONSE] Success | Length: {Length}", text?.Length ?? 0);
            return text ?? "No response from AI.";
        }
        catch (GeminiRateLimitException)
        {
            throw; // Propagate for Worker retry
        }
        catch (HttpRequestException)
        {
            throw; // Propagate HTTP errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GEMINI_EXCEPTION] Error generating chat response");
            throw; // Propagate instead of swallowing
        }
    }

    private List<double> GenerateMockEmbedding()
    {
        // Generate a random 768-dimensional vector
        // Note: text-embedding-004 produces 768-dimensional embeddings
        var random = new Random();
        var embedding = new List<double>();
        for (int i = 0; i < 768; i++)
        {
            embedding.Add(random.NextDouble());
        }
        return embedding;
    }

    public async Task<SkillAnalysisDto?> AnalyzeGitHubSkillsAsync(
        string gitHubUsername,
        Dictionary<string, double> languagePercentages,
        int totalRepos,
        int totalStars,
        int totalContributions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var languageBreakdown = string.Join("\n",
                languagePercentages
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(10)
                    .Select(kvp => $"- {kvp.Key}: {kvp.Value:F2}%"));

            var prompt = $@"
Analyze this GitHub developer profile and extract structured skill information:

**GitHub Statistics:**
- Username: {gitHubUsername}
- Total Repositories: {totalRepos}
- Total Stars: {totalStars}
- Total Contributions: {totalContributions}

**Programming Languages (by usage %):**
{languageBreakdown}

Please provide a JSON response with this exact structure (no additional text):
{{
  ""primarySkills"": [""skill1"", ""skill2"", ""skill3""],
  ""expertiseLevel"": ""Junior/Mid/Senior/Expert"",
  ""specializations"": [""Web Development"", ""Backend"", etc],
  ""skillProficiency"": {{
    ""C#"": ""Advanced"",
    ""TypeScript"": ""Intermediate""
  }},
  ""recommendations"": [""skill1"", ""skill2""],
  ""summary"": ""Brief developer profile summary""
}}
";

            var systemPrompt = "You are a technical recruiter and skills analyst. Analyze GitHub profiles and return ONLY valid JSON responses.";

            _logger.LogInformation("[GEMINI_ANALYSIS_START] Analyzing skills for {Username}", gitHubUsername);
            var response = await GenerateChatResponseAsync(systemPrompt, prompt);

            // Try to extract JSON from response (Gemini might add extra text)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogDebug("[GEMINI_JSON] Extracted: {Json}", jsonText);

                var skillAnalysis = JsonSerializer.Deserialize<SkillAnalysisDto>(jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (skillAnalysis != null && skillAnalysis.PrimarySkills.Count > 0)
                {
                    _logger.LogInformation("[GEMINI_ANALYSIS_SUCCESS] Skills: {Skills}",
                        string.Join(", ", skillAnalysis.PrimarySkills));
                    return skillAnalysis;
                }
            }

            _logger.LogWarning("[GEMINI_PARSE_FAILED] Could not extract valid JSON. Response: {Response}",
                response.Length > 200 ? response.Substring(0, 200) + "..." : response);

            // Fallback to mock analysis (only for JSON parse failures, not API errors)
            return GenerateMockAnalysis(gitHubUsername, languagePercentages, totalRepos, totalStars);
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("[GEMINI_ANALYSIS_429] Rate limited during skill analysis for {Username}", gitHubUsername);
            throw; // Propagate for Worker retry
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[GEMINI_ANALYSIS_HTTP_ERROR] HTTP error during skill analysis");
            throw; // Propagate for Worker retry
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("API Key"))
        {
            _logger.LogWarning("[GEMINI_NO_KEY] API key missing, using mock analysis");
            return GenerateMockAnalysis(gitHubUsername, languagePercentages, totalRepos, totalStars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GEMINI_SKILL_ANALYSIS_FAILED] Error analyzing GitHub skills");
            throw; // Propagate for Worker retry
        }
    }

    private SkillAnalysisDto GenerateMockAnalysis(
        string gitHubUsername,
        Dictionary<string, double> languagePercentages,
        int totalRepos,
        int totalStars)
    {
        _logger.LogInformation("[MOCK_ANALYSIS] Generating mock analysis for {Username}", gitHubUsername);

        // Extract top languages as skills
        var topLanguages = languagePercentages
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => kvp.Key)
            .ToList();

        // Determine expertise level based on stats
        var expertiseLevel = totalRepos switch
        {
            < 5 => "Junior",
            < 15 => "Mid",
            < 30 => "Senior",
            _ => "Expert"
        };

        // Basic specializations based on languages
        var specializations = new List<string>();
        if (topLanguages.Any(l => l.Equals("C#", StringComparison.OrdinalIgnoreCase) ||
                                   l.Equals("Java", StringComparison.OrdinalIgnoreCase)))
            specializations.Add("Backend Development");
        if (topLanguages.Any(l => l.Equals("JavaScript", StringComparison.OrdinalIgnoreCase) ||
                                   l.Equals("TypeScript", StringComparison.OrdinalIgnoreCase)))
            specializations.Add("Web Development");
        if (!specializations.Any())
            specializations.Add("Software Development");

        return new SkillAnalysisDto
        {
            PrimarySkills = topLanguages,
            ExpertiseLevel = expertiseLevel,
            Specializations = specializations,
            SkillProficiency = topLanguages.ToDictionary(
                lang => lang,
                lang => languagePercentages[lang] > 30 ? "Advanced" : "Intermediate"
            ),
            Recommendations = new List<string> { "Docker", "Git", "CI/CD" },
            Summary = $"{expertiseLevel} developer with {totalRepos} repositories and {totalStars} stars. " +
                     $"Primary focus on {string.Join(", ", topLanguages.Take(3))}."
        };
    }

    public async Task<string> GenerateCVHtmlAsync(
        CVGenerationContext ctx,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Gemini API Key is missing.");

        var skillsLine   = ctx.Skills.Any()         ? string.Join(", ", ctx.Skills)         : "N/A";
        var languageLine = ctx.Languages.Any()       ? string.Join(", ", ctx.Languages)      : "";
        var certLine     = ctx.Certifications.Any()  ? string.Join(", ", ctx.Certifications) : "";

        var systemPrompt = @"You are a professional CV/resume designer. Your task is to generate a complete, 
self-contained HTML document representing a modern, professional CV. 
Return ONLY the raw HTML document starting with <!DOCTYPE html>. 
No markdown fences, no extra explanation — just the HTML file content.";

        // Build JD section for the prompt (only when a target job is set)
        var jdSection = string.Empty;
        if (ctx.HasTargetJob)
        {
            var jdSkills = ctx.TargetJobSkillIds.Any() ? string.Join(", ", ctx.TargetJobSkillIds) : "";
            jdSection = $"""

TARGET JOB (tailor the CV to match this posting):
Position: {ctx.TargetJobTitle}
{(string.IsNullOrWhiteSpace(ctx.TargetJobDescription) ? "" : $"Job Description:\n{ctx.TargetJobDescription}")}
{(string.IsNullOrWhiteSpace(jdSkills) ? "" : $"Required Skills: {jdSkills}")}

IMPORTANT TAILORING INSTRUCTIONS:
- In the Professional Summary section, explicitly mention interest in the position "{ctx.TargetJobTitle}".
- Reorder and emphasize skills so the ones matching the job's requirements appear first.
- In the Experience section, highlight achievements and technologies most relevant to this role.
- If the candidate has matching skills/experience, add a small "Why I'm a great fit" note at the top of the Summary.
""";
        }

        var userPrompt = $@"Generate a complete, ATS-optimized, STAR-format HTML CV for the following candidate.

━━━ CANDIDATE DATA ━━━
Name: {ctx.FullName}
Email: {ctx.Email}
Phone: {(string.IsNullOrWhiteSpace(ctx.Phone) ? "N/A" : ctx.Phone)}
Location: {(string.IsNullOrWhiteSpace(ctx.Location) ? (string.IsNullOrWhiteSpace(ctx.Address) ? "N/A" : ctx.Address) : ctx.Location)}
Title: {(string.IsNullOrWhiteSpace(ctx.Title) ? "Software Developer" : ctx.Title)}
Bio/Summary: {(string.IsNullOrWhiteSpace(ctx.Bio) ? "Experienced professional." : ctx.Bio)}
LinkedIn: {(string.IsNullOrWhiteSpace(ctx.LinkedInUrl) ? "" : ctx.LinkedInUrl)}
GitHub: {(string.IsNullOrWhiteSpace(ctx.GitHubUrl) ? "" : ctx.GitHubUrl)}
Portfolio: {(string.IsNullOrWhiteSpace(ctx.Portfolio) ? "" : ctx.Portfolio)}

━━━ EXPERIENCE ━━━
{(string.IsNullOrWhiteSpace(ctx.Experience) ? "Not provided" : ctx.Experience)}

━━━ EDUCATION ━━━
{(string.IsNullOrWhiteSpace(ctx.Education) ? "Not provided" : ctx.Education)}

━━━ SKILLS ━━━
Technical Skills: {skillsLine}
{(string.IsNullOrWhiteSpace(languageLine) ? "" : $"Programming Languages: {languageLine}")}
{(string.IsNullOrWhiteSpace(certLine) ? "" : $"Certifications: {certLine}")}
{jdSection}
━━━ ATS & STAR REQUIREMENTS ━━━
You MUST follow ALL of these rules:

**ATS OPTIMIZATION (Applicant Tracking System):**
1. Use standard section headings: 'Professional Summary', 'Work Experience', 'Education', 'Technical Skills', 'Certifications'
2. Do NOT use tables, columns, text boxes, headers/footers, or images for main content — ATS parsers cannot read them
3. Use a clean linear layout (single column) for the main body
4. Write skills as keyword-rich plain text (not only icons or logos)
5. Include the job title from the candidate's current/latest role prominently
6. Quantify achievements wherever possible: use numbers, percentages, dollar amounts

**STAR FORMAT for every Experience bullet point:**
Each bullet MUST follow: Situation/Task → Action → Result
Format: 'Action verb + what you did + using what tools/method + measurable result'
Examples:
✅ 'Reduced API response time by 40% by migrating to Redis caching, improving user satisfaction scores'
✅ 'Led a team of 5 engineers to deliver a microservices rewrite 2 weeks ahead of schedule'
✅ 'Automated CI/CD pipeline using GitHub Actions, cutting deployment time from 2 hours to 8 minutes'
❌ 'Responsible for developing APIs' (no result, no metric)
❌ 'Worked on frontend' (vague, no action, no result)

**PROFESSIONAL SUMMARY (3-4 sentences):**
- Start with years of experience + specialization
- Include 2-3 core technical strengths (use actual skill keywords)
- End with a value statement or career goal
- If tailored to a JD, mention the target role explicitly

**HTML OUTPUT REQUIREMENTS:**
1. Use a clean, modern layout with a tasteful accent color (#005f73 or similar)
2. Inline ALL CSS — fully self-contained, no external stylesheets
3. ATS-safe: use semantic HTML (h1, h2, p, ul, li) not tables for layout
4. Good typography via Google Fonts @import
5. Print-ready: fits on 1-2 A4 pages, proper margins
6. Return complete HTML starting from <!DOCTYPE html>
NO markdown fences, NO explanations — just the HTML.";


        _logger.LogInformation("[GEMINI_CV_GEN] Generating CV HTML for {Name}", ctx.FullName);

        var htmlContent = await GenerateChatResponseAsync(systemPrompt, userPrompt);

        // Strip markdown fences if Gemini wraps it
        var htmlStart = htmlContent.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        if (htmlStart > 0)
            htmlContent = htmlContent[htmlStart..];

        var htmlEnd = htmlContent.LastIndexOf("</html>", StringComparison.OrdinalIgnoreCase);
        if (htmlEnd > 0)
            htmlContent = htmlContent[..(htmlEnd + 7)];

        _logger.LogInformation("[GEMINI_CV_GEN] Generated HTML CV: {Length} chars", htmlContent.Length);

        return htmlContent;
    }

    /// <summary>
    /// Generate structured CV data (JSON) — editable on frontend, PDF generated client-side
    /// </summary>
    public async Task<CVStructuredData> GenerateCVDataAsync(
        CVGenerationContext ctx,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Gemini API Key is missing.");

        var skillsLine   = ctx.Skills.Any()        ? string.Join(", ", ctx.Skills)        : "N/A";
        var languageLine = ctx.Languages.Any()      ? string.Join(", ", ctx.Languages)     : "";
        var certLine     = ctx.Certifications.Any() ? string.Join(", ", ctx.Certifications): "";

        var jdSection = string.Empty;
        if (ctx.HasTargetJob)
        {
            var jdSkills = ctx.TargetJobSkillIds.Any() ? string.Join(", ", ctx.TargetJobSkillIds) : "";
            jdSection = $"""

TARGET JOB (tailor the CV to match this posting):
Position: {ctx.TargetJobTitle}
{(string.IsNullOrWhiteSpace(ctx.TargetJobDescription) ? "" : $"Job Description:\n{ctx.TargetJobDescription}")}
{(string.IsNullOrWhiteSpace(jdSkills) ? "" : $"Required Skills: {jdSkills}")}
""";
        }

        var systemPrompt = "You are a professional CV writer API. Return ONLY raw JSON — no markdown fences, no explanation.";

        var userPrompt = $@"Generate a structured, ATS-optimized CV for the following candidate using STAR-format experience bullets.

CANDIDATE:
Name: {ctx.FullName}
Email: {ctx.Email}
Phone: {ctx.Phone ?? ""}
Location: {ctx.Location ?? ctx.Address ?? ""}
Title: {ctx.Title ?? "Software Developer"}
Summary/Bio: {ctx.Bio ?? ""}
LinkedIn: {ctx.LinkedInUrl ?? ""}
GitHub: {ctx.GitHubUrl ?? ""}
Portfolio: {ctx.Portfolio ?? ""}

EXPERIENCE (raw input): {ctx.Experience ?? "Not provided"}
EDUCATION (raw input):  {ctx.Education ?? "Not provided"}
SKILLS: {skillsLine}
{(string.IsNullOrWhiteSpace(languageLine) ? "" : $"LANGUAGES: {languageLine}")}
{(string.IsNullOrWhiteSpace(certLine) ? "" : $"CERTIFICATIONS: {certLine}")}
{jdSection}

INSTRUCTIONS:
1. Professional Summary: Write a detailed 4-5 sentence summary. State the candidate's core strengths, years of experience, and notable specializations. 
{(ctx.HasTargetJob ? $"   CRITICAL: This summary MUST explicitly argue why the candidate is the perfect fit for '{ctx.TargetJobTitle}'. Weave the candidate's skills and the job's Required Skills together in a compelling narrative." : "")}
2. Experience bullets: STAR format — Action verb + what + how/tools + measurable result. Expand each bullet point with high professional detail.
{(ctx.HasTargetJob ? "   CRITICAL: Focus heavily on mapping the candidate's past projects to the TARGET JOB's description. Use the exact keywords found in the Target Job." : "")}
3. Skills: ATS-friendly plain list of technical skills. 
{(ctx.HasTargetJob ? "   CRITICAL: Reorder the skills array so the exact ones matching the TARGET JOB appear first." : "")}
4. Synthesize all provided unstructured data into a highly polished, professional persona. Do not invent fake jobs, but elevate the phrasing.

Return EXACTLY this JSON structure (no extra keys, no markdown):
{{
  ""fullName"": ""{ctx.FullName}"",
  ""title"": ""{ctx.TargetJobTitle ?? ctx.Title ?? "Software Developer"}"",
  ""email"": ""{ctx.Email}"",
  ""phone"": ""{ctx.Phone ?? ""}"",
  ""location"": ""{ctx.Location ?? ctx.Address ?? ""}"",
  ""linkedIn"": ""{ctx.LinkedInUrl ?? ""}"",
  ""gitHub"": ""{ctx.GitHubUrl ?? ""}"",
  ""portfolio"": ""{ctx.Portfolio ?? ""}"",
  ""summary"": ""<a rich, compelling, detailed 4-5 sentence professional summary>"",
  ""experience"": [
    {{
      ""company"": ""Company Name"",
      ""role"": ""Job Title"",
      ""period"": ""Jan 2022 – Present"",
      ""bullets"": [""STAR bullet 1"", ""STAR bullet 2""]
    }}
  ],
  ""education"": [
    {{
      ""institution"": ""University Name"",
      ""degree"": ""Bachelor of Computer Science"",
      ""period"": ""2018 – 2022"",
      ""notes"": """"
    }}
  ],
  ""skills"": [""Skill1"", ""Skill2""],
  ""languages"": [],
  ""certifications"": [],
  ""targetJobTitle"": {(ctx.HasTargetJob ? $"\"{ctx.TargetJobTitle}\"" : "null")}
}}";

        _logger.LogInformation("[GEMINI_CV_DATA] Generating structured CV JSON for {Name}", ctx.FullName);

        var raw = await GenerateChatResponseAsync(systemPrompt, userPrompt);

        // Strip markdown fences
        var start = raw.IndexOf('{');
        var end   = raw.LastIndexOf('}');
        if (start < 0 || end <= start)
            throw new InvalidOperationException("Gemini did not return valid JSON for CV data.");

        var json = raw[start..(end + 1)];

        var data = System.Text.Json.JsonSerializer.Deserialize<CVStructuredData>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (data == null)
            throw new InvalidOperationException("Could not deserialize Gemini CV JSON response.");

        _logger.LogInformation("[GEMINI_CV_DATA] Generated structured CV with {ExpCount} experience entries",
            data.Experience.Count);

        return data;
    }

    /// <summary>
    /// RAG: Send top candidate summaries + company request to Gemini for ranking & reasoning
    /// </summary>
    public async Task<List<CandidateRankResult>> RankCandidatesAsync(
        string companyRequest,
        List<CandidateSummaryForRanking> candidates,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("Gemini API Key is missing.");

        if (candidates.Count == 0)
            return new List<CandidateRankResult>();

        // Build candidate summaries for the prompt
        var candidateLines = candidates.Select((c, idx) =>
        {
            var skills = c.Skills.Any() ? string.Join(", ", c.Skills) : "N/A";
            return $"""
            Candidate #{idx + 1}:
            - UserId: {c.UserId}
            - Name: {c.FullName}
            - Title: {c.Title ?? "N/A"}
            - Bio: {(string.IsNullOrWhiteSpace(c.Bio) ? "N/A" : c.Bio)}
            - Experience: {(string.IsNullOrWhiteSpace(c.Experience) ? "N/A" : c.Experience)}
            - Location: {c.Location ?? "N/A"}
            - Skills: {skills}
            - Vector Match Score: {c.VectorScore:F1}%
            """;
        });

        var candidateBlock = string.Join("\n", candidateLines);

        var systemPrompt = @"You are an expert AI recruiting assistant. 
Your task is to analyze candidate profiles and rank them against a company's hiring request.
For each candidate, provide:
1. A brief reason explaining WHY they are (or aren't) a good fit
2. An adjusted match score (0-100) that reflects your analysis

Return ONLY a valid JSON array — no markdown fences, no extra text.
Each element must have: ""userId"" (string), ""reason"" (string, 1-2 sentences), ""adjustedScore"" (number 0-100).
Order by adjustedScore descending.";

        var userPrompt = $@"COMPANY REQUEST:
{companyRequest}

CANDIDATE PROFILES:
{candidateBlock}

Analyze each candidate against the company request. Return a JSON array:
[
  {{""userId"": ""..."", ""reason"": ""..."", ""adjustedScore"": 85}},
  ...
]";

        _logger.LogInformation("[GEMINI_RAG] Ranking {Count} candidates for request: {Request}",
            candidates.Count, companyRequest.Length > 100 ? companyRequest[..100] + "..." : companyRequest);

        try
        {
            var raw = await GenerateChatResponseAsync(systemPrompt, userPrompt);

            // Extract JSON array from response
            var arrayStart = raw.IndexOf('[');
            var arrayEnd = raw.LastIndexOf(']');
            if (arrayStart < 0 || arrayEnd <= arrayStart)
            {
                _logger.LogWarning("[GEMINI_RAG_PARSE] Could not find JSON array in response. Length: {Len}", raw.Length);
                return new List<CandidateRankResult>();
            }

            var jsonArray = raw[arrayStart..(arrayEnd + 1)];

            var results = JsonSerializer.Deserialize<List<RagRankItem>>(jsonArray,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (results == null || results.Count == 0)
            {
                _logger.LogWarning("[GEMINI_RAG_PARSE] Deserialized null/empty from Gemini RAG response");
                return new List<CandidateRankResult>();
            }

            _logger.LogInformation("[GEMINI_RAG] Successfully ranked {Count} candidates", results.Count);

            return results.Select(r => new CandidateRankResult(
                r.UserId ?? "",
                r.Reason ?? "",
                Math.Clamp(r.AdjustedScore, 0, 100)
            )).ToList();
        }
        catch (GeminiRateLimitException)
        {
            _logger.LogWarning("[GEMINI_RAG_429] Rate limited during candidate ranking");
            throw; // Propagate for caller to handle gracefully
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GEMINI_RAG_ERROR] Error ranking candidates");
            // Return empty — caller will use vector-only results
            return new List<CandidateRankResult>();
        }
    }

    /// <summary>
    /// Internal DTO for deserializing Gemini RAG ranking JSON response
    /// </summary>
    private class RagRankItem
    {
        public string? UserId { get; set; }
        public string? Reason { get; set; }
        public double AdjustedScore { get; set; }
    }
}
