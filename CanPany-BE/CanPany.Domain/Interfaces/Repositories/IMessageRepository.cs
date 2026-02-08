using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Message entity
/// </summary>
public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50);
    Task<Message> AddAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(string id);
    Task MarkAsReadAsync(string messageId);
    Task<long> MarkConversationAsReadAsync(string conversationId, string readByUserId);
    Task<long> GetUnreadCountAsync(string conversationId, string userId);
}


