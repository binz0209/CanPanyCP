using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CanPany.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time user-to-user messaging.
/// Clients join conversation groups and receive instant messages, typing indicators, and read receipts.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IConversationRepository _conversationRepo;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IMessageService messageService,
        IConversationRepository conversationRepo,
        ILogger<ChatHub> logger)
    {
        _messageService = messageService;
        _conversationRepo = conversationRepo;
        _logger = logger;
    }

    /// <summary>
    /// Extract the authenticated user's ID from the JWT "sub" claim.
    /// </summary>
    private string GetUserId()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new HubException("Unauthorized: missing user identity.");
        return userId;
    }

    // ─── Connection lifecycle ──────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        // Add the connection to a personal group so we can push notifications
        // (e.g. new conversation created) even when the user hasn't joined a specific conversation.
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        _logger.LogInformation("User {UserId} connected to ChatHub (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} disconnected from ChatHub (ConnectionId: {ConnectionId})", userId, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // ─── Conversation group management ─────────────────────────────────────────

    /// <summary>
    /// Join a conversation group. Only participants of the conversation are allowed.
    /// </summary>
    public async Task JoinConversation(string conversationId)
    {
        var userId = GetUserId();
        var conversation = await _conversationRepo.GetByIdAsync(conversationId);
        if (conversation == null || !conversation.ParticipantIds.Contains(userId))
        {
            throw new HubException("You are not a participant of this conversation.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogDebug("User {UserId} joined conversation {ConversationId}", userId, conversationId);
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogDebug("User {UserId} left conversation {ConversationId}", GetUserId(), conversationId);
    }

    // ─── Messaging ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Send a message to a conversation. The message is persisted and broadcast to all participants.
    /// </summary>
    public async Task SendMessage(string conversationId, string text)
    {
        var userId = GetUserId();

        // Verify the user belongs to this conversation
        var conversation = await _conversationRepo.GetByIdAsync(conversationId);
        if (conversation == null || !conversation.ParticipantIds.Contains(userId))
        {
            throw new HubException("You are not a participant of this conversation.");
        }

        // Persist the message (encryption handled inside MessageService)
        var message = await _messageService.SendAsync(conversationId, userId, text);

        // Broadcast the message to all clients in the conversation group
        await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Text,
            message.IsRead,
            message.CreatedAt
        });

        // Also notify participants' personal groups so their conversation list updates
        foreach (var participantId in conversation.ParticipantIds)
        {
            await Clients.Group($"user_{participantId}").SendAsync("ConversationUpdated", new
            {
                ConversationId = conversationId,
                LastMessagePreview = text.Length > 100 ? text[..100] + "…" : text,
                LastMessageAt = message.CreatedAt,
                SenderId = userId
            });
        }
    }

    // ─── Read receipts ─────────────────────────────────────────────────────────

    /// <summary>
    /// Mark all messages in a conversation as read by the current user.
    /// </summary>
    public async Task MarkAsRead(string conversationId)
    {
        var userId = GetUserId();
        var count = await _messageService.MarkConversationAsReadAsync(conversationId, userId);

        if (count > 0)
        {
            // Notify others in the conversation that messages have been read
            await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("MessageRead", new
            {
                ConversationId = conversationId,
                ReadByUserId = userId,
                Count = count
            });
        }
    }

    // ─── Typing indicator ──────────────────────────────────────────────────────

    /// <summary>
    /// Broadcast a typing indicator to other participants in the conversation.
    /// </summary>
    public async Task Typing(string conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", new
        {
            ConversationId = conversationId,
            UserId = userId
        });
    }
}
