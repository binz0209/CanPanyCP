using CanPany.Domain.DTOs.Analysis;
using CanPany.Domain.DTOs.GitHub;
using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// GitHub Analysis Result - RAG (Retrieval-Augmented Generation) Storage
/// Stores raw GitHub data + AI-analyzed skills for knowledge retrieval
/// </summary>
[BsonIgnoreExtraElements]
public class GitHubAnalysisResult : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>
    /// User ID this analysis belongs to
    /// </summary>
    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// GitHub username analyzed
    /// </summary>
    [BsonElement("gitHubUsername")]
    public string GitHubUsername { get; set; } = null!;

    /// <summary>
    /// Job ID that generated this analysis
    /// </summary>
    [BsonElement("jobId")]
    public string? JobId { get; set; }

    #region RAG: Retrieved Data (from GitHub API)

    /// <summary>
    /// Total repositories count
    /// </summary>
    [BsonElement("totalRepositories")]
    public int TotalRepositories { get; set; }

    /// <summary>
    /// Total contributions/commits
    /// </summary>
    [BsonElement("totalContributions")]
    public int TotalContributions { get; set; }

    /// <summary>
    /// Total stars received
    /// </summary>
    [BsonElement("totalStars")]
    public int TotalStars { get; set; }

    /// <summary>
    /// Total forks
    /// </summary>
    [BsonElement("totalForks")]
    public int TotalForks { get; set; }

    /// <summary>
    /// Language usage statistics (language -> bytes)
    /// </summary>
    [BsonElement("languageBytes")]
    public Dictionary<string, long> LanguageBytes { get; set; } = new();

    /// <summary>
    /// Language percentages (calculated)
    /// </summary>
    [BsonElement("languagePercentages")]
    public Dictionary<string, double> LanguagePercentages { get; set; } = new();

    /// <summary>
    /// Top repositories (summary)
    /// </summary>
    [BsonElement("topRepositories")]
    public List<GitHubRepositorySummary> TopRepositories { get; set; } = new();

    #endregion

    #region RAG: Augmented Data (AI-generated insights)

    /// <summary>
    /// AI-analyzed skill data from Gemini
    /// </summary>
    [BsonElement("skillAnalysis")]
    public SkillAnalysisDto? SkillAnalysis { get; set; }

    /// <summary>
    /// Primary skills extracted (quick access)
    /// </summary>
    [BsonElement("primarySkills")]
    public List<string> PrimarySkills { get; set; } = new();

    /// <summary>
    /// Expertise level (Junior/Mid/Senior/Expert)
    /// </summary>
    [BsonElement("expertiseLevel")]
    public string? ExpertiseLevel { get; set; }

    /// <summary>
    /// Technical specializations
    /// </summary>
    [BsonElement("specializations")]
    public List<string> Specializations { get; set; } = new();

    /// <summary>
    /// AI-generated summary
    /// </summary>
    [BsonElement("aiSummary")]
    public string? AiSummary { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// When this analysis was performed
    /// </summary>
    [BsonElement("analyzedAt")]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Analysis version (for future schema changes)
    /// </summary>
    [BsonElement("analysisVersion")]
    public int AnalysisVersion { get; set; } = 1;

    /// <summary>
    /// Whether this analysis is active/latest
    /// </summary>
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Include forked repos in analysis
    /// </summary>
    [BsonElement("includeForkedRepos")]
    public bool IncludeForkedRepos { get; set; }

    /// <summary>
    /// Raw analysis data (JSON) for future reference
    /// </summary>
    [BsonElement("rawData")]
    public string? RawData { get; set; }

    #endregion

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Simplified repository summary for storage
/// </summary>
public class GitHubRepositorySummary
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("starsCount")]
    public int StarsCount { get; set; }

    [BsonElement("forksCount")]
    public int ForksCount { get; set; }

    [BsonElement("htmlUrl")]
    public string HtmlUrl { get; set; } = null!;

    [BsonElement("isFork")]
    public bool IsFork { get; set; }
}
