using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Message entity
/// </summary>
public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByConversationKeyAsync(string conversationKey);
    Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId);
    Task<IEnumerable<Message>> GetByReceiverIdAsync(string receiverId);
    Task<Message> AddAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(string id);
    Task MarkAsReadAsync(string messageId);
    Task MarkConversationAsReadAsync(string conversationKey, string userId);
}


