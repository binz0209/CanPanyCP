using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CVRepository : ICVRepository
{
    private readonly IMongoCollection<CV> _collection;

    public CVRepository(MongoDbContext context)
    {
        _collection = context.CVs;
    }

    public async Task<CV?> GetByIdAsync(string id)
    {
        return await _collection.Find(cv => cv.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CV>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(cv => cv.UserId == userId).ToListAsync();
    }

    public async Task<CV?> GetDefaultByUserIdAsync(string userId)
    {
        return await _collection.Find(cv => cv.UserId == userId && cv.IsDefault).FirstOrDefaultAsync();
    }

    public async Task<CV> AddAsync(CV cv)
    {
        await _collection.InsertOneAsync(cv);
        return cv;
    }

    public async Task UpdateAsync(CV cv)
    {
        cv.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == cv.Id, cv);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(cv => cv.Id == id);
    }

    public async Task SetAsDefaultAsync(string cvId, string userId)
    {
        // Unset all defaults for this user
        var updateUnset = Builders<CV>.Update.Set(c => c.IsDefault, false);
        await _collection.UpdateManyAsync(c => c.UserId == userId, updateUnset);

        // Set this CV as default
        var updateSet = Builders<CV>.Update.Set(c => c.IsDefault, true);
        await _collection.UpdateOneAsync(c => c.Id == cvId, updateSet);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(cv => cv.Id == id);
        return count > 0;
    }
}

