using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// AI Chat service implementation for career advisor
/// </summary>
public class AIChatService : IAIChatService
{
    private readonly IGeminiService _geminiService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ILogger<AIChatService> _logger;

    public AIChatService(
        IGeminiService geminiService,
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        ILogger<AIChatService> logger)
    {
        _geminiService = geminiService;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _logger = logger;
    }

    public async Task<string> ChatAsync(string userId, string message, string? conversationId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));

            Conversation conversation;
            if (string.IsNullOrEmpty(conversationId))
            {
                // Create a new conversation for AI chat (using 'System_AI' as the second participant)
                conversation = new Conversation
                {
                    ParticipantIds = new List<string> { userId, "System_AI" }.OrderBy(id => id).ToList(),
                    LastMessageAt = DateTime.UtcNow,
                    LastMessagePreview = message.Length > 50 ? message.Substring(0, 50) + "..." : message
                };
                conversation = await _conversationRepository.AddAsync(conversation);
                conversationId = conversation.Id;
            }
            else
            {
                conversation = await _conversationRepository.GetByIdAsync(conversationId)
                    ?? throw new InvalidOperationException("Conversation not found");
                
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.LastMessagePreview = message.Length > 50 ? message.Substring(0, 50) + "..." : message;
                conversation.MarkAsUpdated();
                await _conversationRepository.UpdateAsync(conversation);
            }

            // Save user message
            var userMsg = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Text = message,
                IsRead = true
            };
            await _messageRepository.AddAsync(userMsg);

            // Generate AI response
            _logger.LogInformation("AI Chat request from user: {UserId}, Message: {Message}", userId, message);
            var systemPrompt = "You are CanPany's AI Career Advisor. Help the candidate with their career questions, CV advice, and job recommendations.";
            
            // In a real implementation, we could fetch previous messages to provide context
            // var history = await _messageRepository.GetByConversationIdAsync(conversationId, 1, 10);
            
            var aiResponseText = await _geminiService.GenerateChatResponseAsync(systemPrompt, message);

            // Save AI message
            var aiMsg = new Message
            {
                ConversationId = conversationId,
                SenderId = "System_AI",
                Text = aiResponseText,
                IsRead = false
            };
            await _messageRepository.AddAsync(aiMsg);

            // Update conversation again with AI reply preview
            conversation.LastMessageAt = DateTime.UtcNow;
            conversation.LastMessagePreview = aiResponseText.Length > 50 ? aiResponseText.Substring(0, 50) + "..." : aiResponseText;
            conversation.MarkAsUpdated();
            await _conversationRepository.UpdateAsync(conversation);

            return aiResponseText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<(string ConversationId, string LastMessage, DateTime LastAt)>> GetConversationsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var conversations = await _conversationRepository.GetByUserIdAsync(userId);
            
            // Filter out non-AI conversations if needed, or assume we return AI ones here
            var aiConversations = conversations.Where(c => c.ParticipantIds.Contains("System_AI"));
            
            return aiConversations.Select(c => (
                ConversationId: c.Id,
                LastMessage: c.LastMessagePreview ?? "",
                LastAt: c.LastMessageAt ?? c.CreatedAt
            )).OrderByDescending(c => c.LastAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations: {UserId}", userId);
            throw;
        }
    }
}


