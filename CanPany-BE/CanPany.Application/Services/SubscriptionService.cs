using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Manages premium subscriptions: purchase via wallet, feature gating, lifecycle.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IUserSubscriptionRepository _subRepo;
    private readonly IPremiumPackageRepository _packageRepo;
    private readonly IWalletService _walletService;
    private readonly IPaymentRepository _paymentRepo;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUserSubscriptionRepository subRepo,
        IPremiumPackageRepository packageRepo,
        IWalletService walletService,
        IPaymentRepository paymentRepo,
        INotificationService notificationService,
        ILogger<SubscriptionService> logger)
    {
        _subRepo = subRepo;
        _packageRepo = packageRepo;
        _walletService = walletService;
        _paymentRepo = paymentRepo;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<UserSubscription?> GetActiveSubscriptionAsync(string userId)
    {
        try
        {
            var sub = await _subRepo.GetActiveByUserIdAsync(userId);
            if (sub == null) return null;

            // Check if expired
            if (sub.EndDate < DateTime.UtcNow)
            {
                sub.Status = "Expired";
                sub.MarkAsUpdated();
                await _subRepo.UpdateAsync(sub);
                return null;
            }

            return sub;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscription for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsPremiumAsync(string userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId);
        return sub != null;
    }

    /// <inheritdoc/>
    public async Task<bool> HasFeatureAsync(string userId, string featureName)
    {
        var sub = await GetActiveSubscriptionAsync(userId);
        if (sub == null) return false;
        // If features list is empty, premium grants all features
        if (sub.Features.Count == 0) return true;
        return sub.Features.Contains(featureName, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<(bool Succeeded, string[] Errors, UserSubscription? Subscription)> PurchasePremiumAsync(
        string userId, string packageId)
    {
        try
        {
            // 1. Get the package
            var package = await _packageRepo.GetByIdAsync(packageId);
            if (package == null || !package.IsActive)
                return (false, new[] { "Gói premium không tồn tại hoặc đã ngừng bán." }, null);

            // 2. Check if user already has active subscription
            var existingSub = await GetActiveSubscriptionAsync(userId);
            if (existingSub != null)
                return (false, new[] { "Bạn đã có gói premium đang hoạt động. Vui lòng đợi hết hạn trước khi mua gói mới." }, existingSub);

            // 3. Check wallet balance
            var balance = await _walletService.GetBalanceAsync(userId);
            if (balance < package.Price)
                return (false, new[] { $"Số dư ví không đủ. Cần {package.Price:N0} VND, hiện có {balance:N0} VND." }, null);

            // 4. Deduct from wallet
            var (deductSucceeded, deductErrors, _) = await _walletService.ChangeBalanceAsync(
                userId, -package.Price, $"Mua gói Premium: {package.Name}");

            if (!deductSucceeded)
                return (false, deductErrors, null);

            // 5. Create Payment record
            var payment = new Payment
            {
                UserId = userId,
                Purpose = "PremiumPurchase",
                PackageId = packageId,
                Amount = package.Price,
                Currency = "VND",
                Status = "Paid",
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            payment = await _paymentRepo.AddAsync(payment);

            // 6. Create UserSubscription
            var subscription = new UserSubscription
            {
                UserId = userId,
                PackageId = packageId,
                PaymentId = payment.Id,
                Status = "Active",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
                Features = package.Features,
                CreatedAt = DateTime.UtcNow
            };
            subscription = await _subRepo.AddAsync(subscription);

            // 7. Send notification
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Type = "PremiumActivated",
                    Title = "Premium đã kích hoạt! 🎉",
                    Message = $"Gói {package.Name} đã được kích hoạt thành công. Hết hạn: {subscription.EndDate:dd/MM/yyyy}.",
                    Payload = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        SubscriptionId = subscription.Id,
                        PackageName = package.Name,
                        EndDate = subscription.EndDate
                    }),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
                await _notificationService.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send premium activation notification for user {UserId}", userId);
            }

            _logger.LogInformation(
                "User {UserId} purchased premium package {PackageName} (ID: {PackageId}), subscription until {EndDate}",
                userId, package.Name, packageId, subscription.EndDate);

            return (true, Array.Empty<string>(), subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing premium for user {UserId}, package {PackageId}", userId, packageId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UserSubscription>> GetSubscriptionHistoryAsync(string userId)
    {
        try
        {
            return await _subRepo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription history for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PremiumPackage>> GetAvailablePackagesAsync()
    {
        try
        {
            var all = await _packageRepo.GetAllAsync();
            return all.Where(p => p.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available packages");
            throw;
        }
    }
}
