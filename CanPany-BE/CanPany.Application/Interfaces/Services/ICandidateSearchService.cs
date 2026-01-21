using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Candidate search service interface
/// </summary>
public interface ICandidateSearchService
{
    Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesAsync(string jobId, int limit = 20);
    Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesSemanticAsync(
        string jobId,
        int limit = 20,
        string? location = null,
        string? experienceLevel = null);
    Task<bool> UnlockCandidateContactAsync(string companyId, string candidateId);
    Task<IEnumerable<(UserProfile Profile, double MatchScore)>> SearchCandidatesWithFiltersAsync(
        string? keyword, 
        List<string>? skillIds, 
        string? location, 
        decimal? minHourlyRate, 
        decimal? maxHourlyRate, 
        int page = 1, 
        int pageSize = 20);
    Task<bool> HasUnlockedCandidateAsync(string companyId, string candidateId);
    Task<IEnumerable<(User User, UserProfile Profile)>> GetUnlockedCandidatesAsync(string companyId, int page = 1, int pageSize = 20);
}


