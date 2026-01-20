using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// User profile service interface
/// </summary>
public interface IUserProfileService
{
    Task<UserProfile?> GetByUserIdAsync(string userId);
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task<bool> UpdateAsync(string userId, UserProfile profile);
    Task<bool> SyncFromLinkedInAsync(string userId, string linkedInData);
    Task<bool> SyncFromGitHubAsync(string userId, string gitHubData);
}


