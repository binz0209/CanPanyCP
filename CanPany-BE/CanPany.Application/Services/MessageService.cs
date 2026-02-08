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

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(string conversationId, int page = 1, int pageSize = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));

            return await _repo.GetByConversationIdAsync(conversationId, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages by conversation ID: {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<Message> SendAsync(string conversationId, string senderId, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
            if (string.IsNullOrWhiteSpace(senderId))
                throw new ArgumentException("Sender ID cannot be null or empty", nameof(senderId));
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text cannot be null or empty", nameof(text));

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Text = text,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

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

    public async Task<long> MarkConversationAsReadAsync(string conversationId, string readByUserId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                throw new ArgumentException("Conversation ID cannot be null or empty", nameof(conversationId));
            if (string.IsNullOrWhiteSpace(readByUserId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(readByUserId));

            return await _repo.MarkConversationAsReadAsync(conversationId, readByUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read: {ConversationId}, {UserId}", conversationId, readByUserId);
            throw;
        }
    }
}

