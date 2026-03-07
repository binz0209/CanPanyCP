using CanPany.Domain.DTOs.Analysis;

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
}
