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

    // Profile extraction fields
    [BsonElement("profile")]
    public ExtractedProfile? Profile { get; set; }

    // Simple string fields for experience and education (for backward compatibility)
    [BsonElement("experience")]
    public string? Experience { get; set; }

    [BsonElement("education")]
    public string? Education { get; set; }

    [BsonElement("experiences")]
    public List<ExtractedExperience> Experiences { get; set; } = new();

    [BsonElement("educations")]
    public List<ExtractedEducation> Educations { get; set; } = new();

    [BsonElement("languages")]
    public List<ExtractedLanguage> Languages { get; set; } = new();

    [BsonElement("certifications")]
    public List<ExtractedCertification> Certifications { get; set; } = new();
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

public class ExtractedProfile
{
    [BsonElement("fullName")]
    public string? FullName { get; set; }

    [BsonElement("email")]
    public string? Email { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("linkedIn")]
    public string? LinkedIn { get; set; }

    [BsonElement("github")]
    public string? GitHub { get; set; }

    [BsonElement("portfolio")]
    public string? Portfolio { get; set; }

    [BsonElement("summary")]
    public string? Summary { get; set; }
}

public class ExtractedExperience
{
    [BsonElement("company")]
    public string? Company { get; set; }

    [BsonElement("position")]
    public string? Position { get; set; }

    [BsonElement("startDate")]
    public string? StartDate { get; set; }

    [BsonElement("endDate")]
    public string? EndDate { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }
}

public class ExtractedEducation
{
    [BsonElement("school")]
    public string? School { get; set; }

    [BsonElement("degree")]
    public string? Degree { get; set; }

    [BsonElement("field")]
    public string? Field { get; set; }

    [BsonElement("startDate")]
    public string? StartDate { get; set; }

    [BsonElement("endDate")]
    public string? EndDate { get; set; }
}

public class ExtractedLanguage
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("level")]
    public string? Level { get; set; }
}

public class ExtractedCertification
{
    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("issuer")]
    public string? Issuer { get; set; }

    [BsonElement("date")]
    public string? Date { get; set; }
}


