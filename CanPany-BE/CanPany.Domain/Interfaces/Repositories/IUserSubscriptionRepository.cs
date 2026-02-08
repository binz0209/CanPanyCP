using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserSubscription entity
/// </summary>
public interface IUserSubscriptionRepository
{
    Task<UserSubscription?> GetByIdAsync(string id);
    Task<IEnumerable<UserSubscription>> GetByUserIdAsync(string userId);
    Task<UserSubscription?> GetActiveByUserIdAsync(string userId);
    Task<UserSubscription> AddAsync(UserSubscription subscription);
    Task UpdateAsync(UserSubscription subscription);
    Task DeleteAsync(string id);
    Task<bool> HasActiveSubscriptionAsync(string userId);
}
