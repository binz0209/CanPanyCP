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
    
    /// <summary>
    /// Search profiles using vector similarity
    /// </summary>
    Task<IEnumerable<(UserProfile Profile, double Score)>> SearchByVectorAsync(List<double> vector, int limit = 20, double minScore = 0.5);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

