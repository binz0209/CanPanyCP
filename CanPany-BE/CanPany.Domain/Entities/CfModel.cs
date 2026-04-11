using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// CF Model entity — Stores trained Collaborative Filtering model snapshots.
/// Enables model versioning, rollback, and audit trail for defense.
/// </summary>
[BsonIgnoreExtraElements]
public class CfModel : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    /// <summary>"user-knn" | "item-knn" | "matrix-factorization" | "hybrid"</summary>
    [BsonElement("modelType")]
    public string ModelType { get; set; } = "user-knn";

    [BsonElement("version")]
    public int Version { get; set; } = 1;

    /// <summary>"training" | "active" | "archived"</summary>
    [BsonElement("status")]
    public string Status { get; set; } = "training";

    // kNN parameters
    [BsonElement("similarityMetric")]
    public string? SimilarityMetric { get; set; } = "cosine";

    [BsonElement("kNeighbors")]
    public int? KNeighbors { get; set; } = 20;

    // Hybrid parameters
    [BsonElement("alphaWeight")]
    public double? AlphaWeight { get; set; }

    // Training metrics
    [BsonElement("trainingMetrics")]
    public CfTrainingMetrics? TrainingMetrics { get; set; }

    [BsonElement("trainedAt")]
    public DateTime? TrainedAt { get; set; }

    [BsonElement("activatedAt")]
    public DateTime? ActivatedAt { get; set; }

    [BsonElement("archivedAt")]
    public DateTime? ArchivedAt { get; set; }

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Training metrics captured during CF model training.
/// </summary>
public class CfTrainingMetrics
{
    [BsonElement("totalUsers")]
    public int TotalUsers { get; set; }

    [BsonElement("totalJobs")]
    public int TotalJobs { get; set; }

    [BsonElement("totalInteractions")]
    public int TotalInteractions { get; set; }

    /// <summary>Percentage of empty cells in user-item matrix</summary>
    [BsonElement("sparsity")]
    public double Sparsity { get; set; }

    [BsonElement("trainingDurationMs")]
    public long TrainingDurationMs { get; set; }
}
