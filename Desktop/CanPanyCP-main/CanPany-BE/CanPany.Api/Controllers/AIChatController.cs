using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// AI Chat controller - UC-CAN-23
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIChatController : ControllerBase
{
    private readonly IAIChatService _aiChatService;
    private readonly ILogger<AIChatController> _logger;

    public AIChatController(
        IAIChatService aiChatService,
        ILogger<AIChatController> logger)
    {
        _aiChatService = aiChatService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-23: Chat with AI Career Advisor
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _aiChatService.ChatAsync(userId, request.Message, request.ConversationId);
            return Ok(ApiResponse<object>.CreateSuccess(new { response, conversationId = request.ConversationId }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI chat");
            return StatusCode(500, ApiResponse.CreateError("Failed to process chat", "ChatFailed"));
        }
    }

    /// <summary>
    /// Get conversation history
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversations = await _aiChatService.GetConversationsAsync(userId);
            return Ok(ApiResponse.CreateSuccess(conversations, "Conversations retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(500, ApiResponse.CreateError("Failed to get conversations", "GetConversationsFailed"));
        }
    }
}

public record ChatRequest(string Message, string? ConversationId = null);


