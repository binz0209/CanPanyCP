using CanPany.Domain.Entities;

namespace CanPany.Application.DTOs;

/// <summary>
/// Request for semantic search based on job description
/// </summary>
public record SemanticSearchRequest(
    string JobDescription,
    string? Location = null,
    string? ExperienceLevel = null,
    int Limit = 20
);

/// <summary>
/// Response for semantic search result
/// </summary>
public record SemanticSearchResponse(
    UserProfileDto Profile,
    double MatchScore,
    UserBasicInfo UserInfo
);
