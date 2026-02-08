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
    /// UC-COM-08: View Message History for a conversation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] string conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(conversationId))
                return BadRequest(ApiResponse.CreateError("Conversation ID is required", "MissingConversationId"));

            var messages = await _messageService.GetByConversationIdAsync(conversationId, page, pageSize);
            return Ok(ApiResponse<IEnumerable<Message>>.CreateSuccess(messages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return StatusCode(500, ApiResponse.CreateError("Failed to get messages", "GetMessagesFailed"));
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

            var sentMessage = await _messageService.SendAsync(request.ConversationId, userId, request.Text);
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
    [HttpPut("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkConversationAsRead(string conversationId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var count = await _messageService.MarkConversationAsReadAsync(conversationId, userId);
            return Ok(ApiResponse.CreateSuccess($"{count} messages marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read");
            return StatusCode(500, ApiResponse.CreateError("Failed to mark conversation as read", "MarkConversationAsReadFailed"));
        }
    }
}

public record SendMessageRequest(string ConversationId, string Text);


