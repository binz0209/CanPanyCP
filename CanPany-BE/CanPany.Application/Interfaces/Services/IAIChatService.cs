namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// AI Chat service interface for career advisor
/// </summary>
public interface IAIChatService
{
    Task<string> ChatAsync(string userId, string message, string? conversationId = null);
    Task<IEnumerable<(string ConversationId, string LastMessage, DateTime LastAt)>> GetConversationsAsync(string userId);
}


