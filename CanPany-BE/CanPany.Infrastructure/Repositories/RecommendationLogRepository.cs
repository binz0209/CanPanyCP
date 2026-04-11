using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class RecommendationLogRepository : IRecommendationLogRepository
{
    private readonly IMongoCollection<RecommendationLog> _collection;

    public RecommendationLogRepository(MongoDbContext context)
    {
        _collection = context.RecommendationLogs;
    }

    public async Task<RecommendationLog?> GetByIdAsync(string id)
    {
        return await _collection.Find(l => l.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RecommendationLog>> GetByUserIdAsync(string userId, int limit = 20)
    {
        return await _collection
            .Find(l => l.UserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<RecommendationLog> AddAsync(RecommendationLog log)
    {
        await _collection.InsertOneAsync(log);
        return log;
    }

    public async Task<long> GetTotalCountAsync()
    {
        return await _collection.CountDocumentsAsync(_ => true);
    }
}
