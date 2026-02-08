using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly IMongoCollection<UserSubscription> _collection;

    public UserSubscriptionRepository(MongoDbContext context)
    {
        _collection = context.UserSubscriptions;
    }

    public async Task<UserSubscription?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserSubscription>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(s => s.UserId == userId)
            .SortByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserSubscription?> GetActiveByUserIdAsync(string userId)
    {
        return await _collection.Find(s => s.UserId == userId && s.Status == "Active" && s.EndDate > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<UserSubscription> AddAsync(UserSubscription subscription)
    {
        await _collection.InsertOneAsync(subscription);
        return subscription;
    }

    public async Task UpdateAsync(UserSubscription subscription)
    {
        subscription.MarkAsUpdated();
        await _collection.ReplaceOneAsync(s => s.Id == subscription.Id, subscription);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<bool> HasActiveSubscriptionAsync(string userId)
    {
        var count = await _collection.CountDocumentsAsync(
            s => s.UserId == userId && s.Status == "Active" && s.EndDate > DateTime.UtcNow);
        return count > 0;
    }
}
