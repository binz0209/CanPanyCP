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

    public async Task<IEnumerable<Message>> GetByConversationKeyAsync(string conversationKey)
    {
        return await _collection.Find(m => m.ConversationKey == conversationKey)
            .SortBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId)
    {
        return await _collection.Find(m => m.SenderId == senderId).ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetByReceiverIdAsync(string receiverId)
    {
        return await _collection.Find(m => m.ReceiverId == receiverId).ToListAsync();
    }

    public async Task<Message> AddAsync(Message message)
    {
        await _collection.InsertOneAsync(message);
        return message;
    }

    public async Task UpdateAsync(Message message)
    {
        message.MarkAsUpdated();
        await _collection.ReplaceOneAsync(m => m.Id == message.Id, message);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(m => m.Id == id);
    }

    public async Task MarkAsReadAsync(string messageId)
    {
        var update = Builders<Message>.Update.Set(m => m.IsRead, true);
        await _collection.UpdateOneAsync(m => m.Id == messageId, update);
    }

    public async Task MarkConversationAsReadAsync(string conversationKey, string userId)
    {
        var update = Builders<Message>.Update.Set(m => m.IsRead, true);
        await _collection.UpdateManyAsync(
            m => m.ConversationKey == conversationKey && m.ReceiverId == userId && !m.IsRead,
            update);
    }
}


