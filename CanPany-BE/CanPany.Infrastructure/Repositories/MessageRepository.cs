using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IMongoCollection<Message> _collection;

    public MessageRepository(MongoDbContext context)
    {
        _collection = context.Messages;
    }

    public async Task<Message?> GetByIdAsync(string id)
    {
        return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50)
    {
        return await _collection.Find(m => m.ConversationId == conversationId)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Message> AddAsync(Message message)
    {
        await _collection.InsertOneAsync(message);
        return message;
    }

    public async Task UpdateAsync(Message message)
    {
        await _collection.ReplaceOneAsync(m => m.Id == message.Id, message);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(m => m.Id == id);
    }

    public async Task MarkAsReadAsync(string messageId)
    {
        var update = Builders<Message>.Update
            .Set(m => m.IsRead, true)
            .Set(m => m.ReadAt, DateTime.UtcNow);
        await _collection.UpdateOneAsync(m => m.Id == messageId, update);
    }

    public async Task<long> MarkConversationAsReadAsync(string conversationId, string readByUserId)
    {
        var update = Builders<Message>.Update
            .Set(m => m.IsRead, true)
            .Set(m => m.ReadAt, DateTime.UtcNow);
        var result = await _collection.UpdateManyAsync(
            m => m.ConversationId == conversationId && m.SenderId != readByUserId && !m.IsRead,
            update);
        return result.ModifiedCount;
    }

    public async Task<long> GetUnreadCountAsync(string conversationId, string userId)
    {
        return await _collection.CountDocumentsAsync(
            m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);
    }

    public async Task<long> GetTotalUnreadCountAsync(string userId)
    {
        return await _collection.CountDocumentsAsync(
            m => m.SenderId != userId && !m.IsRead);
    }
}


