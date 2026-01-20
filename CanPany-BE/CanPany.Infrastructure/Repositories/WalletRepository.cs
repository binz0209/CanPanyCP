using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly IMongoCollection<Wallet> _collection;
    private readonly ILogger<WalletRepository>? _logger;

    public WalletRepository(MongoDbContext context, ILogger<WalletRepository>? logger = null)
    {
        _collection = context.Wallets;
        _logger = logger;
    }

    public async Task<Wallet?> GetByIdAsync(string id)
    {
        try
        {
            return await _collection.Find(w => w.Id == id).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting wallet by ID: {WalletId}", id);
            throw;
        }
    }

    public async Task<Wallet?> GetByUserIdAsync(string userId)
    {
        try
        {
            return await _collection.Find(w => w.UserId == userId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting wallet by UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<Wallet> AddAsync(Wallet wallet)
    {
        return await _collection.InsertOneWithVerificationAsync(wallet, _logger, "Wallet");
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        await _collection.ReplaceOneWithVerificationAsync(
            Builders<Wallet>.Filter.Eq(w => w.Id, wallet.Id),
            wallet,
            _logger,
            "Wallet");
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneWithVerificationAsync(
            Builders<Wallet>.Filter.Eq(w => w.Id, id),
            _logger,
            "Wallet",
            id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            var count = await _collection.CountDocumentsAsync(w => w.Id == id);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if wallet exists. Id: {WalletId}", id);
            throw;
        }
    }
}


