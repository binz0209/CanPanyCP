using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Messages controller - UC-COM-07, UC-COM-08, UC-COM-09
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(
        IMessageService messageService,
        ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _logger = logger;
    }

    /// <summary>
    /// UC-COM-08: View Message History
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] string? conversationKey)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            IEnumerable<Message> messages;
            if (!string.IsNullOrWhiteSpace(conversationKey))
            {
                messages = await _messageService.GetByConversationKeyAsync(conversationKey);
            }
            else
            {
                messages = await _messageService.GetByUserIdAsync(userId);
            }

            return Ok(ApiResponse<IEnumerable<Message>>.CreateSuccess(messages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return StatusCode(500, ApiResponse.CreateError("Failed to get messages", "GetMessagesFailed"));
        }
    }

    /// <summary>
    /// Get conversations list
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversations = await _messageService.GetConversationsForUserAsync(userId);
            return Ok(ApiResponse.CreateSuccess(conversations));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(500, ApiResponse.CreateError("Failed to get conversations", "GetConversationsFailed"));
        }
    }

    /// <summary>
    /// UC-COM-07: Send Message
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = new Message
            {
                SenderId = userId,
                ReceiverId = request.ReceiverId,
                ProjectId = request.ProjectId,
                Text = request.Text,
                CreatedAt = DateTime.UtcNow
            };

            var sentMessage = await _messageService.SendAsync(message);
            return Ok(ApiResponse<Message>.CreateSuccess(sentMessage, "Message sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, ApiResponse.CreateError("Failed to send message", "SendMessageFailed"));
        }
    }

    /// <summary>
    /// UC-COM-09: Mark Message as Read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        try
        {
            var succeeded = await _messageService.MarkAsReadAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Message not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Message marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            return StatusCode(500, ApiResponse.CreateError("Failed to mark message as read", "MarkAsReadFailed"));
        }
    }

    /// <summary>
    /// Mark conversation as read
    /// </summary>
    [HttpPut("conversations/{conversationKey}/read")]
    public async Task<IActionResult> MarkConversationAsRead(string conversationKey)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _messageService.MarkConversationAsReadAsync(conversationKey, userId);
            return Ok(ApiResponse.CreateSuccess("Conversation marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read");
            return StatusCode(500, ApiResponse.CreateError("Failed to mark conversation as read", "MarkConversationAsReadFailed"));
        }
    }
}

public record SendMessageRequest(string ReceiverId, string Text, string? ProjectId = null);


