using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// User Premium service interface
/// </summary>
public interface IUserPremiumService
{
    Task<bool> CheckUserPremiumAsync(string userId);
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);
    Task<UserSubscription> PurchasePackageAsync(string userId, string packageId);
}
