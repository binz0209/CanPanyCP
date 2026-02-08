using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly IMongoCollection<Conversation> _collection;

    public ConversationRepository(MongoDbContext context)
    {
        _collection = context.Conversations;
    }

    public async Task<Conversation?> GetByIdAsync(string id)
    {
        return await _collection.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Conversation?> GetByParticipantsAsync(string userId1, string userId2, string? jobId = null)
    {
        var sortedIds = new List<string> { userId1, userId2 }.OrderBy(x => x).ToList();
        var filter = Builders<Conversation>.Filter.And(
            Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, sortedIds[0]),
            Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, sortedIds[1]),
            Builders<Conversation>.Filter.Size(c => c.ParticipantIds, 2)
        );

        if (jobId != null)
        {
            filter = Builders<Conversation>.Filter.And(filter,
                Builders<Conversation>.Filter.Eq(c => c.JobId, jobId));
        }

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
    {
        return await _collection.Find(c => c.ParticipantIds.Contains(userId))
            .SortByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Conversation> AddAsync(Conversation conversation)
    {
        await _collection.InsertOneAsync(conversation);
        return conversation;
    }

    public async Task UpdateAsync(Conversation conversation)
    {
        conversation.MarkAsUpdated();
        await _collection.ReplaceOneAsync(c => c.Id == conversation.Id, conversation);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(c => c.Id == id);
    }
}
