using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Wallet entity
/// </summary>
public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(string id);
    Task<Wallet?> GetByUserIdAsync(string userId);
    Task<Wallet> AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Atomically change the wallet balance using MongoDB $inc.
    /// For deductions (negative delta), also checks that balance + delta >= 0 in the filter
    /// to prevent going below zero.
    /// Returns the updated Wallet, or null if user not found / insufficient balance.
    /// </summary>
    Task<Wallet?> AtomicChangeBalanceAsync(string userId, long delta);
}


