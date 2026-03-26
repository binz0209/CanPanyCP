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

    public async Task<FilterPreset> AddAsync(FilterPreset filterPreset)
    {
        await _collection.InsertOneAsync(filterPreset);
        return filterPreset;
    }

    public async Task UpdateAsync(FilterPreset filterPreset)
    {
        filterPreset.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(fp => fp.Id == filterPreset.Id, filterPreset);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(fp => fp.Id == id);
    }
}
