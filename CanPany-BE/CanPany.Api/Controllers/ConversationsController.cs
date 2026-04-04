using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Conversations controller — list, create, and manage conversations between users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IUserRepository _userRepo;
    private readonly II18nService _i18nService;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IConversationRepository conversationRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepo,
        II18nService i18nService,
        ILogger<ConversationsController> logger)
    {
        _conversationRepo = conversationRepo;
        _messageRepo = messageRepo;
        _userRepo = userRepo;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// Get all conversations for the current user (paginated, sorted by latest message).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversations = await _conversationRepo.GetByUserIdAsync(userId, page, pageSize);
            var result = new List<ConversationDto>();

            foreach (var conv in conversations)
            {
                var otherUserId = conv.ParticipantIds.FirstOrDefault(id => id != userId);
                var otherUser = otherUserId != null ? await _userRepo.GetByIdAsync(otherUserId) : null;
                var unreadCount = await _messageRepo.GetUnreadCountAsync(conv.Id, userId);

                result.Add(new ConversationDto
                {
                    Id = conv.Id,
                    ParticipantIds = conv.ParticipantIds,
                    OtherUserName = otherUser?.FullName ?? "Unknown",
                    OtherUserAvatar = otherUser?.AvatarUrl,
                    JobId = conv.JobId,
                    LastMessageAt = conv.LastMessageAt,
                    LastMessagePreview = conv.LastMessagePreview,
                    UnreadCount = unreadCount,
                    CreatedAt = conv.CreatedAt
                });
            }

            return Ok(ApiResponse<List<ConversationDto>>.CreateSuccess(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetConversationsFailed"));
        }
    }

    /// <summary>
    /// Get or create a conversation between the current user and another user.
    /// Idempotent — returns the existing conversation if one already exists.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GetOrCreateConversation([FromBody] CreateConversationRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.OtherUserId))
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Conversation.OtherUserRequired), "MissingOtherUserId"));

            if (userId == request.OtherUserId)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "SelfConversation"));

            // Check if the other user exists
            var otherUser = await _userRepo.GetByIdAsync(request.OtherUserId);
            if (otherUser == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Conversation.UserNotFound), "UserNotFound"));

            // Try to find an existing conversation
            var conversation = await _conversationRepo.GetByParticipantsAsync(userId, request.OtherUserId, request.JobId);

            if (conversation == null)
            {
                // Create a new conversation
                var sortedIds = new List<string> { userId, request.OtherUserId }.OrderBy(x => x).ToList();
                conversation = new Conversation
                {
                    ParticipantIds = sortedIds,
                    JobId = request.JobId,
                    CreatedAt = DateTime.UtcNow
                };
                conversation = await _conversationRepo.AddAsync(conversation);
                _logger.LogInformation("Created conversation {ConversationId} between {User1} and {User2}",
                    conversation.Id, userId, request.OtherUserId);
            }

            var unreadCount = await _messageRepo.GetUnreadCountAsync(conversation.Id, userId);

            var dto = new ConversationDto
            {
                Id = conversation.Id,
                ParticipantIds = conversation.ParticipantIds,
                OtherUserName = otherUser.FullName,
                OtherUserAvatar = otherUser.AvatarUrl,
                JobId = conversation.JobId,
                LastMessageAt = conversation.LastMessageAt,
                LastMessagePreview = conversation.LastMessagePreview,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt
            };

            return Ok(ApiResponse<ConversationDto>.CreateSuccess(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateConversationFailed"));
        }
    }

    /// <summary>
    /// Get a single conversation by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversation(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversation = await _conversationRepo.GetByIdAsync(id);
            if (conversation == null || !conversation.ParticipantIds.Contains(userId))
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Conversation.NotFound), "NotFound"));

            var otherUserId = conversation.ParticipantIds.FirstOrDefault(pid => pid != userId);
            var otherUser = otherUserId != null ? await _userRepo.GetByIdAsync(otherUserId) : null;
            var unreadCount = await _messageRepo.GetUnreadCountAsync(conversation.Id, userId);

            var dto = new ConversationDto
            {
                Id = conversation.Id,
                ParticipantIds = conversation.ParticipantIds,
                OtherUserName = otherUser?.FullName ?? "Unknown",
                OtherUserAvatar = otherUser?.AvatarUrl,
                JobId = conversation.JobId,
                LastMessageAt = conversation.LastMessageAt,
                LastMessagePreview = conversation.LastMessagePreview,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt
            };

            return Ok(ApiResponse<ConversationDto>.CreateSuccess(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation {ConversationId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetConversationFailed"));
        }
    }

    /// <summary>
    /// Get total unread message count across all conversations.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _messageRepo.GetTotalUnreadCountAsync(userId);
            return Ok(ApiResponse<long>.CreateSuccess(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetUnreadCountFailed"));
        }
    }
}

// ─── DTOs ────────────────────────────────────────────────────────────────────────

public class ConversationDto
{
    public string Id { get; set; } = string.Empty;
    public List<string> ParticipantIds { get; set; } = new();
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatar { get; set; }
    public string? JobId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
    public long UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateConversationRequest(string OtherUserId, string? JobId = null);
