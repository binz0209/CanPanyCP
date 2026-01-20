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
}


