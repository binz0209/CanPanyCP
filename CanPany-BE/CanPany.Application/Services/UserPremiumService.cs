using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// User Premium service implementation
/// </summary>
public class UserPremiumService : IUserPremiumService
{
    private readonly IUserSubscriptionRepository _subscriptionRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly IPremiumPackageRepository _packageRepo;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<UserPremiumService> _logger;

    public UserPremiumService(
        IUserSubscriptionRepository subscriptionRepo,
        IWalletRepository walletRepo,
        IPremiumPackageRepository packageRepo,
        IUserRepository userRepo,
        ILogger<UserPremiumService> logger)
    {
        _subscriptionRepo = subscriptionRepo;
        _walletRepo = walletRepo;
        _packageRepo = packageRepo;
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<bool> CheckUserPremiumAsync(string userId)
    {
        try
        {
            var activeSubscription = await _subscriptionRepo.GetActiveByUserIdAsync(userId);
            return activeSubscription != null && activeSubscription.EndDate > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user premium for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        var sub = await _subscriptionRepo.GetActiveByUserIdAsync(userId);
        if (sub != null && sub.EndDate > DateTime.UtcNow)
        {
            return sub;
        }
        return null;
    }

    public async Task<UserSubscription> PurchasePackageAsync(string userId, string packageId)
    {
        // 1. Get User and Wallet
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var wallet = await _walletRepo.GetByUserIdAsync(userId);
        if (wallet == null) throw new Exception("Wallet not found");

        // 2. Get Package
        var package = await _packageRepo.GetByIdAsync(packageId);
        if (package == null || !package.IsActive) throw new Exception("Package not found or inactive");

        // 3. Check Balance
        if (wallet.Balance < package.Price)
            throw new Exception("Bạn không đủ số dư để mua gói này. Vui lòng nạp thêm tiền.");

        // 4. Deduct Balance
        wallet.Balance -= package.Price;
        await _walletRepo.UpdateAsync(wallet);

        // 5. Create Subscription
        var subscription = new UserSubscription
        {
            UserId = userId,
            PackageId = packageId,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
            Features = package.Features,
            CreatedAt = DateTime.UtcNow
        };

        return await _subscriptionRepo.AddAsync(subscription);
    }
}
