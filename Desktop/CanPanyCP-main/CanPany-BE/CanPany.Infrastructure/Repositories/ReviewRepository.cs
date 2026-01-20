using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly IMongoCollection<Review> _collection;

    public ReviewRepository(MongoDbContext context)
    {
        _collection = context.Reviews;
    }

    public async Task<Review?> GetByIdAsync(string id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Review>> GetByProjectIdAsync(string projectId)
    {
        return await _collection.Find(r => r.ProjectId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId)
    {
        return await _collection.Find(r => r.ReviewerId == reviewerId).ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(string revieweeId)
    {
        return await _collection.Find(r => r.RevieweeId == revieweeId).ToListAsync();
    }

    public async Task<Review> AddAsync(Review review)
    {
        await _collection.InsertOneAsync(review);
        return review;
    }

    public async Task UpdateAsync(Review review)
    {
        review.MarkAsUpdated();
        await _collection.ReplaceOneAsync(r => r.Id == review.Id, review);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(r => r.Id == id);
        return count > 0;
    }
}

