using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Message service interface
/// </summary>
public interface IMessageService
{
    Task<Message?> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50);
    Task<Message> SendAsync(string conversationId, string senderId, string text);
    Task<bool> MarkAsReadAsync(string messageId);
    Task<long> MarkConversationAsReadAsync(string conversationId, string readByUserId);
}


