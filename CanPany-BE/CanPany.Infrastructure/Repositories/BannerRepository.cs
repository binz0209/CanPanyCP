using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly IMongoCollection<Banner> _collection;

    public BannerRepository(MongoDbContext context)
    {
        _collection = context.Banners;
    }

    public async Task<Banner?> GetByIdAsync(string id)
    {
        return await _collection.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Banner>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortBy(b => b.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
    {
        return await _collection.Find(b => b.IsActive)
            .SortBy(b => b.Order)
            .ToListAsync();
    }

    public async Task<Banner> AddAsync(Banner banner)
    {
        await _collection.InsertOneAsync(banner);
        return banner;
    }

    public async Task UpdateAsync(Banner banner)
    {
        banner.MarkAsUpdated();
        await _collection.ReplaceOneAsync(b => b.Id == banner.Id, banner);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(b => b.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(b => b.Id == id);
        return count > 0;
    }
}


