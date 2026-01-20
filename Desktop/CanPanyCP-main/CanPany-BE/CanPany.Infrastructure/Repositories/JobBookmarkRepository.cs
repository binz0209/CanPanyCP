using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class JobBookmarkRepository : IJobBookmarkRepository
{
    private readonly IMongoCollection<JobBookmark> _collection;

    public JobBookmarkRepository(MongoDbContext context)
    {
        _collection = context.JobBookmarks;
    }

    public async Task<JobBookmark?> GetByIdAsync(string id)
    {
        return await _collection.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<JobBookmark?> GetByUserAndJobAsync(string userId, string jobId)
    {
        return await _collection.Find(b => b.UserId == userId && b.JobId == jobId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<JobBookmark>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(b => b.UserId == userId)
            .SortByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<JobBookmark> AddAsync(JobBookmark bookmark)
    {
        await _collection.InsertOneAsync(bookmark);
        return bookmark;
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(b => b.Id == id);
    }

    public async Task<bool> ExistsAsync(string userId, string jobId)
    {
        var count = await _collection.CountDocumentsAsync(b => b.UserId == userId && b.JobId == jobId);
        return count > 0;
    }
}


