using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly IMongoCollection<UserSettings> _collection;

    public UserSettingsRepository(MongoDbContext context)
    {
        _collection = context.UserSettings;
    }

    public async Task<UserSettings?> GetByIdAsync(string id)
    {
        return await _collection.Find(us => us.Id == id).FirstOrDefaultAsync();
    }

    public async Task<UserSettings?> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(us => us.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<UserSettings> AddAsync(UserSettings userSettings)
    {
        await _collection.InsertOneAsync(userSettings);
        return userSettings;
    }

    public async Task UpdateAsync(UserSettings userSettings)
    {
        userSettings.MarkAsUpdated();
        await _collection.ReplaceOneAsync(us => us.Id == userSettings.Id, userSettings);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(us => us.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(us => us.Id == id);
        return count > 0;
    }

    public async Task<bool> ExistsByUserIdAsync(string userId)
    {
        var count = await _collection.CountDocumentsAsync(us => us.UserId == userId);
        return count > 0;
    }
}

