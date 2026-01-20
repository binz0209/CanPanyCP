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
    Task<UserProfile> AddAsync(UserProfile profile);
    Task UpdateAsync(UserProfile profile);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

