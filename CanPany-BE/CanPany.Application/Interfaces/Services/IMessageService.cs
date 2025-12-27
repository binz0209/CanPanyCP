using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Message service interface
/// </summary>
public interface IMessageService
{
    Task<Message?> GetByIdAsync(string id);
    Task<IEnumerable<Message>> GetByConversationKeyAsync(string conversationKey);
    Task<IEnumerable<Message>> GetByUserIdAsync(string userId);
    Task<IEnumerable<(string ConversationKey, string PartnerId, string LastMessage, DateTime LastAt, int UnreadCount)>> GetConversationsForUserAsync(string userId);
    Task<Message> SendAsync(Message message);
    Task<bool> MarkAsReadAsync(string messageId);
    Task<int> MarkConversationAsReadAsync(string conversationKey, string userId);
}


