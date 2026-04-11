using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class CfModelRepository : ICfModelRepository
{
    private readonly IMongoCollection<CfModel> _collection;

    public CfModelRepository(MongoDbContext context)
    {
        _collection = context.CfModels;
    }

    public async Task<CfModel?> GetByIdAsync(string id)
    {
        return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<CfModel?> GetActiveModelAsync()
    {
        return await _collection
            .Find(m => m.Status == "active")
            .SortByDescending(m => m.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<CfModel?> GetLatestByTypeAsync(string modelType)
    {
        return await _collection
            .Find(m => m.ModelType == modelType)
            .SortByDescending(m => m.Version)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CfModel>> GetAllAsync()
    {
        return await _collection
            .Find(_ => true)
            .SortByDescending(m => m.Version)
            .ToListAsync();
    }

    public async Task<int> GetNextVersionAsync()
    {
        var latest = await _collection
            .Find(_ => true)
            .SortByDescending(m => m.Version)
            .FirstOrDefaultAsync();

        return (latest?.Version ?? 0) + 1;
    }

    public async Task<CfModel> AddAsync(CfModel model)
    {
        await _collection.InsertOneAsync(model);
        return model;
    }

    public async Task UpdateAsync(CfModel model)
    {
        model.MarkAsUpdated();
        await _collection.ReplaceOneAsync(m => m.Id == model.Id, model);
    }

    public async Task ArchiveAllActiveAsync()
    {
        var filter = Builders<CfModel>.Filter.Eq(m => m.Status, "active");
        var update = Builders<CfModel>.Update
            .Set(m => m.Status, "archived")
            .Set(m => m.ArchivedAt, DateTime.UtcNow);

        await _collection.UpdateManyAsync(filter, update);
    }
}
