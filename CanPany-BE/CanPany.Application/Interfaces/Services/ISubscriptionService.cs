using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service managing premium subscriptions - purchasing with wallet balance,
/// checking active features, and subscription lifecycle.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Get the current active subscription for a user (null if none/expired)
    /// </summary>
    Task<UserSubscription?> GetActiveSubscriptionAsync(string userId);

    /// <summary>
    /// Check if a user has an active premium subscription
    /// </summary>
    Task<bool> IsPremiumAsync(string userId);

    /// <summary>
    /// Check if a user has access to a specific premium feature
    /// </summary>
    Task<bool> HasFeatureAsync(string userId, string featureName);

    /// <summary>
    /// Purchase a premium package using wallet balance.
    /// Deducts from wallet → creates Payment → creates UserSubscription.
    /// </summary>
    Task<(bool Succeeded, string[] Errors, UserSubscription? Subscription)> PurchasePremiumAsync(
        string userId, string packageId);

    /// <summary>
    /// Get all subscription history for a user
    /// </summary>
    Task<IEnumerable<UserSubscription>> GetSubscriptionHistoryAsync(string userId);

    /// <summary>
    /// Get available premium packages (filtered by active status and optionally by userType)
    /// </summary>
    Task<IEnumerable<PremiumPackage>> GetAvailablePackagesAsync(string? userType = null);
}
