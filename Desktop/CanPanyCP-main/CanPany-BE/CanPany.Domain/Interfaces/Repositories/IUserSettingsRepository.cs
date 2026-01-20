using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserSettings entity
/// </summary>
public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByIdAsync(string id);
    Task<UserSettings?> GetByUserIdAsync(string userId);
    Task<UserSettings> AddAsync(UserSettings userSettings);
    Task UpdateAsync(UserSettings userSettings);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsByUserIdAsync(string userId);
}

