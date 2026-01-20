using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly IMongoCollection<WalletTransaction> _collection;

    public WalletTransactionRepository(MongoDbContext context)
    {
        _collection = context.WalletTransactions;
    }

    public async Task<WalletTransaction?> GetByIdAsync(string id)
    {
        return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(string walletId)
    {
        return await _collection.Find(t => t.WalletId == walletId)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WalletTransaction>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(t => t.UserId == userId)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<WalletTransaction> AddAsync(WalletTransaction transaction)
    {
        await _collection.InsertOneAsync(transaction);
        return transaction;
    }

    public async Task UpdateAsync(WalletTransaction transaction)
    {
        transaction.MarkAsUpdated();
        await _collection.ReplaceOneAsync(t => t.Id == transaction.Id, transaction);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(t => t.Id == id);
    }
}


