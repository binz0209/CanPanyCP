using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserProfile entity
/// </summary>
public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(string id);
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task<IEnumerable<UserProfile>> GetProfilesCreatedAfterAsync(DateTime date);
    
    /// <summary>
    /// Search profiles using vector similarity
    /// </summary>
    Task<IEnumerable<(UserProfile Profile, double Score)>> SearchByVectorAsync(List<double> vector, int limit = 20, double minScore = 0.0);

    /// <summary>
    /// Search profiles using MongoDB filters (text search, skill match, location, hourly rate)
    /// No embeddings required — pure DB query
    /// </summary>
    Task<IEnumerable<UserProfile>> SearchByFiltersAsync(
        string? keyword = null,
        List<string>? skillIds = null,
        string? location = null,
        string? experience = null,
        decimal? minHourlyRate = null,
        decimal? maxHourlyRate = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get profiles that have any of the specified skills
    /// </summary>
    Task<IEnumerable<UserProfile>> GetBySkillIdsAsync(List<string> skillIds, int limit = 20);

    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

