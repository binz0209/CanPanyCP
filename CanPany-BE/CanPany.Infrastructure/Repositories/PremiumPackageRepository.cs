using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class PremiumPackageRepository : IPremiumPackageRepository
{
    private readonly IMongoCollection<PremiumPackage> _collection;

    public PremiumPackageRepository(MongoDbContext context)
    {
        _collection = context.PremiumPackages;
    }

    public async Task<PremiumPackage?> GetByIdAsync(string id)
    {
        return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<PremiumPackage>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<PremiumPackage>> GetActivePackagesAsync()
    {
        return await _collection.Find(p => p.IsActive).ToListAsync();
    }

    public async Task<PremiumPackage> AddAsync(PremiumPackage package)
    {
        await _collection.InsertOneAsync(package);
        return package;
    }

    public async Task UpdateAsync(PremiumPackage package)
    {
        package.MarkAsUpdated();
        await _collection.ReplaceOneAsync(p => p.Id == package.Id, package);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(p => p.Id == id);
        return count > 0;
    }
}


