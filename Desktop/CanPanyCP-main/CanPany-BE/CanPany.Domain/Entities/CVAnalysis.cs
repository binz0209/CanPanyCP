using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// CVAnalysis entity - Represents AI analysis results of a CV
/// </summary>
[BsonIgnoreExtraElements]
public class CVAnalysis : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = string.Empty;

    [BsonElement("cvId"), BsonRepresentation(BsonType.ObjectId)]
    public string CVId { get; set; } = string.Empty;

    [BsonElement("candidateId"), BsonRepresentation(BsonType.ObjectId)]
    public string CandidateId { get; set; } = string.Empty;

    [BsonElement("atsScore")]
    public double ATSScore { get; set; } // 0-100

    [BsonElement("scoreBreakdown")]
    public ATSScoreBreakdown ScoreBreakdown { get; set; } = new();

    [BsonElement("extractedSkills")]
    public ExtractedSkills ExtractedSkills { get; set; } = new();

    [BsonElement("missingKeywords")]
    public List<string> MissingKeywords { get; set; } = new();

    [BsonElement("improvementSuggestions")]
    public List<string> ImprovementSuggestions { get; set; } = new();

    [BsonElement("starRewrittenSections")]
    public Dictionary<string, string> STARRewrittenSections { get; set; } = new();

    [BsonElement("analyzedAt")]
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ATSScoreBreakdown
{
    [BsonElement("keywords")]
    public double Keywords { get; set; }

    [BsonElement("formatting")]
    public double Formatting { get; set; }

    [BsonElement("skills")]
    public double Skills { get; set; }

    [BsonElement("experience")]
    public double Experience { get; set; }

    [BsonElement("education")]
    public double Education { get; set; }
}

public class ExtractedSkills
{
    [BsonElement("technical")]
    public List<string> Technical { get; set; } = new();

    [BsonElement("soft")]
    public List<string> Soft { get; set; } = new();
}


