using System.Net;
using System.Text;
using System.Text.Json;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.DTOs.Analysis;
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
    private readonly string _embeddingModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent";
    private readonly string _chatModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";

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

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
                return response;

            // Parse Retry-After header (seconds) or use exponential backoff
            double retryAfterSeconds = Math.Pow(2, attempt) * BaseRetryDelaySeconds;
            if (response.Headers.RetryAfter?.Delta != null)
            {
                retryAfterSeconds = response.Headers.RetryAfter.Delta.Value.TotalSeconds;
            }

            _logger.LogWarning(
                "[GEMINI_429] Rate limited. Attempt {Attempt}/{MaxRetries}. Waiting {Seconds}s before retry",
                attempt + 1, MaxRetries, retryAfterSeconds);

            if (attempt == MaxRetries)
            {
                throw new GeminiRateLimitException((int)retryAfterSeconds);
            }

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
            var requestBody = new
            {
                model = "models/text-embedding-004",
                content = new { parts = new[] { new { text = text } } }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Use retry-aware send
            var response = await SendWithRetryAsync($"{_embeddingModelUrl}?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();

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
}
