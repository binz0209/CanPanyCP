using CanPany.Application.Models;
using CanPany.Domain.DTOs.Analysis;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IGeminiService
{
    Task<List<double>> GenerateEmbeddingAsync(string text);
    Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage);

    /// <summary>
    /// Analyze GitHub data and extract skills using AI
    /// </summary>
    Task<SkillAnalysisDto?> AnalyzeGitHubSkillsAsync(
        string gitHubUsername,
        Dictionary<string, double> languagePercentages,
        int totalRepos,
        int totalStars,
        int totalContributions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate structured CV data (JSON) from candidate profile — editable on frontend, PDF on download
    /// </summary>
    Task<CVStructuredData> GenerateCVDataAsync(
        CVGenerationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a professionally-designed HTML CV document from candidate profile data (legacy)
    /// </summary>
    Task<string> GenerateCVHtmlAsync(
        CVGenerationContext context,
        CancellationToken cancellationToken = default);
}
