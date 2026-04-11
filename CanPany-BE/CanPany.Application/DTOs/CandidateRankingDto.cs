namespace CanPany.Application.DTOs;

/// <summary>
/// Summarized candidate data sent to Gemini for RAG ranking
/// </summary>
public record CandidateSummaryForRanking(
    string UserId,
    string FullName,
    string? Title,
    string? Bio,
    string? Experience,
    string? Location,
    List<string> Skills,
    double VectorScore
);

/// <summary>
/// Per-candidate result returned by Gemini RAG ranking
/// </summary>
public record CandidateRankResult(
    string UserId,
    string Reason,
    double AdjustedScore
);
