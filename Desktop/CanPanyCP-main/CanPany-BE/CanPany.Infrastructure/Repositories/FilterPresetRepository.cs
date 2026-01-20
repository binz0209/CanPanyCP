using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class FilterPresetRepository : IFilterPresetRepository
{
    private readonly IMongoCollection<FilterPreset> _collection;

    public FilterPresetRepository(MongoDbContext context)
    {
        _collection = context.FilterPresets;
    }

    public async Task<FilterPreset?> GetByIdAsync(string id)
    {
        return await _collection.Find(fp => fp.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(fp => fp.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<FilterPreset>> GetByUserIdAndTypeAsync(string userId, string filterType)
    {
        return await _collection.Find(fp => fp.UserId == userId && fp.FilterType == filterType).ToListAsync();
    }

    public async Task<FilterPreset> AddAsync(FilterPreset filterPreset)
    {
        await _collection.InsertOneAsync(filterPreset);
        return filterPreset;
    }

    public async Task UpdateAsync(FilterPreset filterPreset)
    {
        filterPreset.MarkAsUpdated();
        await _collection.ReplaceOneAsync(fp => fp.Id == filterPreset.Id, filterPreset);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(fp => fp.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(fp => fp.Id == id);
        return count > 0;
    }
}

