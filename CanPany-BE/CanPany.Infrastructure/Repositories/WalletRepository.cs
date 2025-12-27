using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly IMongoCollection<Wallet> _collection;

    public WalletRepository(MongoDbContext context)
    {
        _collection = context.Wallets;
    }

    public async Task<Wallet?> GetByIdAsync(string id)
    {
        return await _collection.Find(w => w.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Wallet?> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(w => w.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<Wallet> AddAsync(Wallet wallet)
    {
        await _collection.InsertOneAsync(wallet);
        return wallet;
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        wallet.MarkAsUpdated();
        await _collection.ReplaceOneAsync(w => w.Id == wallet.Id, wallet);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(w => w.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(w => w.Id == id);
        return count > 0;
    }
}


