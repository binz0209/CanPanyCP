using CanPany.Application.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.DTOs.Analysis;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

public interface IGeminiService
{
    Task<List<double>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch embedding — sends multiple texts in a single API call to reduce rate-limit pressure.
    /// Falls back to sequential single calls if batch endpoint is unavailable.
    /// </summary>
    Task<List<List<double>>> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default);

    Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// RAG: Rank and explain candidate matches against a company's search request.
    /// Returns per-candidate AI reasoning and adjusted scores.
    /// </summary>
    Task<List<CandidateRankResult>> RankCandidatesAsync(
        string companyRequest,
        List<CandidateSummaryForRanking> candidates,
        CancellationToken cancellationToken = default);
}

