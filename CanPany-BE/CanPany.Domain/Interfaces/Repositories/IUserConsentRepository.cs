using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserConsent entity
/// </summary>
public interface IUserConsentRepository
{
    Task<UserConsent?> GetByIdAsync(string id);
    Task<IEnumerable<UserConsent>> GetByUserIdAsync(string userId);
    Task<UserConsent?> GetByUserAndTypeAsync(string userId, string consentType);
    Task<UserConsent> AddAsync(UserConsent consent);
    Task UpdateAsync(UserConsent consent);
    Task<bool> HasConsentAsync(string userId, string consentType);
}
