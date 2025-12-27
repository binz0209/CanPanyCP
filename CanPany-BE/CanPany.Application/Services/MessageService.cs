using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Message service implementation
/// </summary>
public class MessageService : IMessageService
{
    private readonly IMessageRepository _repo;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IMessageRepository repo,
        ILogger<MessageService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Message?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Message ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message by ID: {MessageId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetByConversationKeyAsync(string conversationKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationKey))
                throw new ArgumentException("Conversation key cannot be null or empty", nameof(conversationKey));

            return await _repo.GetByConversationKeyAsync(conversationKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages by conversation key: {ConversationKey}", conversationKey);
            throw;
        }
    }

    public async Task<IEnumerable<Message>> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var sent = await _repo.GetBySenderIdAsync(userId);
            var received = await _repo.GetByReceiverIdAsync(userId);
            return sent.Concat(received).OrderBy(m => m.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<(string ConversationKey, string PartnerId, string LastMessage, DateTime LastAt, int UnreadCount)>> GetConversationsForUserAsync(string userId)
    {
        try
        {
            // TODO: Implement conversation grouping logic
            // This should group messages by conversation key and return latest message per conversation
            throw new NotImplementedException("GetConversationsForUserAsync not fully implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<Message> SendAsync(Message message)
    {
        try
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Build conversation key if not provided
            if (string.IsNullOrWhiteSpace(message.ConversationKey))
            {
                var u1 = string.CompareOrdinal(message.SenderId, message.ReceiverId) <= 0 ? message.SenderId : message.ReceiverId;
                var u2 = u1 == message.SenderId ? message.ReceiverId : message.SenderId;
                var projectId = message.ProjectId ?? "null";
                message.ConversationKey = $"{projectId}:{u1}:{u2}";
            }

            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;
            return await _repo.AddAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(string messageId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageId))
                throw new ArgumentException("Message ID cannot be null or empty", nameof(messageId));

            await _repo.MarkAsReadAsync(messageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read: {MessageId}", messageId);
            throw;
        }
    }

    public async Task<int> MarkConversationAsReadAsync(string conversationKey, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationKey))
                throw new ArgumentException("Conversation key cannot be null or empty", nameof(conversationKey));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            await _repo.MarkConversationAsReadAsync(conversationKey, userId);
            return 0; // TODO: Return actual count
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read: {ConversationKey}, {UserId}", conversationKey, userId);
            throw;
        }
    }
}

