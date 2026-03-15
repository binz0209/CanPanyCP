using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class UserJobInteractionRepository : IUserJobInteractionRepository
{
    private readonly IMongoCollection<UserJobInteraction> _collection;

    public UserJobInteractionRepository(MongoDbContext context)
    {
        _collection = context.UserJobInteractions;
    }

    public async Task<UserJobInteraction?> GetByIdAsync(string id)
    {
        return await _collection.Find(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UserJobInteraction>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(i => i.UserId == userId)
            .SortByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserJobInteraction>> GetByJobIdAsync(string jobId)
    {
        return await _collection.Find(i => i.JobId == jobId).ToListAsync();
    }

    public async Task<UserJobInteraction?> GetByUserJobAndTypeAsync(string userId, string jobId, InteractionType type)
    {
        return await _collection.Find(i => i.UserId == userId && i.JobId == jobId && i.Type == type)
            .FirstOrDefaultAsync();
    }

    public async Task<UserJobInteraction> AddAsync(UserJobInteraction interaction)
    {
        await _collection.InsertOneAsync(interaction);
        return interaction;
    }

    public async Task<IEnumerable<UserJobInteraction>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<long> GetCountByUserIdAsync(string userId)
    {
        return await _collection.CountDocumentsAsync(i => i.UserId == userId);
    }

    public async Task<IEnumerable<string>> GetDistinctUserIdsAsync()
    {
        var filter = Builders<UserJobInteraction>.Filter.Empty;
        return await _collection.Distinct<string>("userId", filter).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctJobIdsByUserAsync(string userId)
    {
        var filter = Builders<UserJobInteraction>.Filter.Eq(i => i.UserId, userId);
        return await _collection.Distinct<string>("jobId", filter).ToListAsync();
    }
}
