using CanPany.Domain.Entities;
using CanPany.Application.DTOs;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Candidate search service interface
/// </summary>
public interface ICandidateSearchService
{
    Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesAsync(string jobId, int limit = 20);
    Task<bool> UnlockCandidateContactAsync(string companyId, string candidateId);
    Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesWithFiltersAsync(
        string? keyword, 
        List<string>? skillIds, 
        string? location, 
        string? experience,
        decimal? minHourlyRate, 
        decimal? maxHourlyRate, 
        int page = 1, 
        int pageSize = 20);
    Task<IEnumerable<SemanticSearchResponse>> SemanticSearchAsync(SemanticSearchRequest request);
    Task<bool> HasUnlockedCandidateAsync(string companyId, string candidateId);
    Task<IEnumerable<(User User, UserProfile Profile)>> GetUnlockedCandidatesAsync(string companyId, int page = 1, int pageSize = 20);
}


