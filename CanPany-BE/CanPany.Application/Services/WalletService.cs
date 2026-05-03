using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Wallet service implementation
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _repo;
    private readonly IWalletTransactionRepository _transactionRepo;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository repo,
        IWalletTransactionRepository transactionRepo,
        ILogger<WalletService> logger)
    {
        _repo = repo;
        _transactionRepo = transactionRepo;
        _logger = logger;
    }

    public async Task<Wallet?> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<long> GetBalanceAsync(string userId)
    {
        try
        {
            var wallet = await EnsureAsync(userId);
            return wallet.Balance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance: {UserId}", userId);
            throw;
        }
    }

    public async Task<Wallet> EnsureAsync(string userId)
    {
        try
        {
            var wallet = await _repo.GetByUserIdAsync(userId);
            if (wallet != null)
                return wallet;

            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };
            return await _repo.AddAsync(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring wallet: {UserId}", userId);
            throw;
        }
    }

    public async Task<(bool Succeeded, string[] Errors, Wallet? Wallet)> ChangeBalanceAsync(string userId, long delta, string? note = null)
    {
        try
        {
            // Ensure wallet exists first
            var wallet = await EnsureAsync(userId);

            // Use atomic operation to prevent race conditions
            var updatedWallet = await _repo.AtomicChangeBalanceAsync(userId, delta);

            if (updatedWallet == null)
            {
                // For negative delta, this means insufficient balance
                // For positive delta, this means wallet not found (shouldn't happen after EnsureAsync)
                return (false, new[] { "Insufficient balance" }, wallet);
            }

            // Record transaction
            var transaction = new WalletTransaction
            {
                WalletId = updatedWallet.Id,
                UserId = userId,
                Type = delta > 0 ? "TopUp" : "Withdraw",
                Amount = Math.Abs(delta),
                BalanceAfter = updatedWallet.Balance,
                Note = note,
                CreatedAt = DateTime.UtcNow
            };
            await _transactionRepo.AddAsync(transaction);

            return (true, Array.Empty<string>(), updatedWallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing balance: {UserId}, {Delta}", userId, delta);
            return (false, new[] { ex.Message }, null);
        }
    }

    public async Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int take = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _transactionRepo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history: {UserId}", userId);
            throw;
        }
    }
}


