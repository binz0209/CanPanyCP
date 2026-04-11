using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// RecommendationLog entity — Logs every recommendation served to users.
/// Enables: (1) CF feedback loop, (2) A/B testing, (3) explainability for defense.
/// </summary>
[BsonIgnoreExtraElements]
public class RecommendationLog : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>"semantic" | "cf" | "hybrid"</summary>
    [BsonElement("recommendationType")]
    public string RecommendationType { get; set; } = "hybrid";

    [BsonElement("recommendedJobIds")]
    public List<string> RecommendedJobIds { get; set; } = new();

    [BsonElement("scores")]
    public List<RecommendationScore> Scores { get; set; } = new();

    [BsonElement("cfModelVersion")]
    public int? CfModelVersion { get; set; }

    [BsonElement("alphaUsed")]
    public double? AlphaUsed { get; set; }

    /// <summary>"profile" | "cv" | "search_query"</summary>
    [BsonElement("inputContext")]
    public string? InputContext { get; set; }

    [BsonElement("totalCandidateJobs")]
    public int TotalCandidateJobs { get; set; }

    [BsonElement("interactionCount")]
    public long InteractionCount { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual job score breakdown in a recommendation.
/// </summary>
public class RecommendationScore
{
    [BsonElement("jobId")]
    public string JobId { get; set; } = string.Empty;

    [BsonElement("semanticScore")]
    public double? SemanticScore { get; set; }

    [BsonElement("cfScore")]
    public double? CfScore { get; set; }

    [BsonElement("hybridScore")]
    public double? HybridScore { get; set; }

    [BsonElement("rank")]
    public int Rank { get; set; }
}
