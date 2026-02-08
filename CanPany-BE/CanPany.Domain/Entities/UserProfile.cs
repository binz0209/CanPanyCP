using CanPany.Shared.Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CanPany.Domain.Entities;

/// <summary>
/// User Profile entity - Extended profile information for Candidates. Aggregate Root.
/// </summary>
[BsonIgnoreExtraElements]
public class UserProfile : AggregateRoot
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public new string Id { get; set; } = null!;

    [BsonElement("userId"), BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("skillIds")]
    public List<string> SkillIds { get; set; } = new();

    [BsonElement("experience")]
    public string? Experience { get; set; }

    [BsonElement("education")]
    public string? Education { get; set; }

    [BsonElement("portfolio")]
    public string? Portfolio { get; set; }

    [BsonElement("linkedInUrl")]
    public string? LinkedInUrl { get; set; }

    [BsonElement("githubUrl")]
    public string? GitHubUrl { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("hourlyRate")]
    public decimal? HourlyRate { get; set; }

    [BsonElement("languages")]
    public List<string> Languages { get; set; } = new();

    [BsonElement("certifications")]
    public List<string> Certifications { get; set; } = new();

    [BsonElement("createdAt")]
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public new DateTime? UpdatedAt { get; set; }

    [BsonElement("embedding")]
    public List<double>? Embedding { get; set; }
}

