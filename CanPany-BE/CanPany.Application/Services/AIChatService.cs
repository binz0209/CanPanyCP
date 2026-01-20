using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// AI Chat service implementation for career advisor
/// </summary>
public class AIChatService : IAIChatService
{
    private readonly ILogger<AIChatService> _logger;
    // TODO: Add GeminiService dependency when available

    public AIChatService(ILogger<AIChatService> logger)
    {
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

            // TODO: Implement AI chat with Gemini API
            // This should use GeminiService to generate career advice responses
            _logger.LogInformation("AI Chat request from user: {UserId}, Message: {Message}", userId, message);
            
            // Placeholder response
            await Task.CompletedTask;
            return "Tôi là AI Career Advisor của CanPany. Tôi sẽ giúp bạn với câu hỏi về sự nghiệp. Tính năng này đang được phát triển.";
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

            // TODO: Implement conversation history retrieval
            // This should retrieve conversation history from database
            await Task.CompletedTask;
            return Enumerable.Empty<(string, string, DateTime)>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations: {UserId}", userId);
            throw;
        }
    }
}


