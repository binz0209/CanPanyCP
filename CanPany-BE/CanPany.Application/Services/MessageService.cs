using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CanPany.Application.Services;

/// <summary>
/// Message service implementation with AES-256 encryption for message text.
/// Automatically updates conversation metadata (lastMessageAt, lastMessagePreview) on send.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IMessageRepository _repo;
    private readonly IConversationRepository _conversationRepo;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<MessageService> _logger;
    private readonly string _encryptionKey;

    public MessageService(
        IMessageRepository repo,
        IConversationRepository conversationRepo,
        IEncryptionService encryptionService,
        IConfiguration configuration,
        ILogger<MessageService> logger)
    {
        _repo = repo;
        _conversationRepo = conversationRepo;
        _encryptionService = encryptionService;
        _logger = logger;
        _encryptionKey = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
    }

    public async Task<Message?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Message ID cannot be null or empty", nameof(id));

            var message = await _repo.GetByIdAsync(id);
            if (message != null)
                DecryptMessage(message);
            return message;
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

            var messages = await _repo.GetByConversationIdAsync(conversationId, page, pageSize);
            foreach (var message in messages)
                DecryptMessage(message);
            return messages;
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

            // Encrypt message text before saving
            var encryptedText = EncryptText(text);

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Text = encryptedText,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            var saved = await _repo.AddAsync(message);

            // Update conversation metadata
            try
            {
                var conversation = await _conversationRepo.GetByIdAsync(conversationId);
                if (conversation != null)
                {
                    conversation.LastMessageAt = saved.CreatedAt;
                    conversation.LastMessagePreview = text.Length > 100 ? text[..100] + "…" : text;
                    conversation.UpdatedAt = DateTime.UtcNow;
                    await _conversationRepo.UpdateAsync(conversation);
                }
            }
            catch (Exception ex)
            {
                // Non-critical: log but don't fail the message send
                _logger.LogWarning(ex, "Failed to update conversation metadata for {ConversationId}", conversationId);
            }

            // Return with decrypted text for the caller
            saved.Text = text;
            return saved;
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

    public async Task<long> GetTotalUnreadCountAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetTotalUnreadCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total unread count for user: {UserId}", userId);
            throw;
        }
    }

    // ==================== Message Encryption Helpers ====================

    private string EncryptText(string text)
    {
        try
        {
            return _encryptionService.Encrypt(text, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt message text");
            return text; // Fallback to plaintext if encryption fails
        }
    }

    private void DecryptMessage(Message message)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
                message.Text = _encryptionService.Decrypt(message.Text, _encryptionKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt message text (may be unencrypted legacy data)");
            // Don't throw - return raw text if decryption fails (legacy data)
        }
    }
}
