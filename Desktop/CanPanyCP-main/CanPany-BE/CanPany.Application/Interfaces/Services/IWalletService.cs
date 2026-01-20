using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Wallet service interface
/// </summary>
public interface IWalletService
{
    Task<Wallet?> GetByUserIdAsync(string userId);
    Task<long> GetBalanceAsync(string userId);
    Task<Wallet> EnsureAsync(string userId);
    Task<(bool Succeeded, string[] Errors, Wallet? Wallet)> ChangeBalanceAsync(string userId, long delta, string? note = null);
    Task<IEnumerable<WalletTransaction>> GetTransactionHistoryAsync(string userId, int take = 20);
}


