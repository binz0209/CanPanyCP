using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for WalletTransaction entity
/// </summary>
public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(string id);
    Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(string walletId);
    Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(string userId);
    Task<WalletTransaction> AddAsync(WalletTransaction transaction);
    Task UpdateAsync(WalletTransaction transaction);
    Task DeleteAsync(string id);
}


