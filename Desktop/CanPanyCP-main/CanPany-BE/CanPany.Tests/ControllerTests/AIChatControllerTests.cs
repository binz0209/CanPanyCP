using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CanPany.Tests.ControllerTests;

public class AIChatControllerTests
{
    private readonly Mock<IAIChatService> _aiChatServiceMock = new();
    private readonly Mock<ILogger<AIChatController>> _loggerMock = new();
    private readonly AIChatController _controller;

    public AIChatControllerTests()
    {
        _controller = new AIChatController(
            _aiChatServiceMock.Object,
            _loggerMock.Object);
        
        // Setup authenticated user
        var claims = new List<Claim> { new Claim("sub", "user123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Chat_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var request = new ChatRequest("What skills do I need for a software engineer role?", "conv123");
        var response = "Based on your profile, you should focus on...";
        
        _aiChatServiceMock.Setup(x => x.ChatAsync(userId, request.Message, request.ConversationId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Chat(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task Chat_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };
        var request = new ChatRequest("Hello", null);

        // Act
        var result = await _controller.Chat(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetConversations_ShouldReturnSuccess_WhenConversationsExist()
    {
        // Arrange
        var userId = "user123";
        var conversations = new List<(string ConversationId, string LastMessage, DateTime LastAt)>
        {
            ("conv1", "Last message 1", DateTime.UtcNow),
            ("conv2", "Last message 2", DateTime.UtcNow.AddHours(-1))
        };
        
        _aiChatServiceMock.Setup(x => x.GetConversationsAsync(userId))
            .ReturnsAsync(conversations);

        // Act
        var result = await _controller.GetConversations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<IEnumerable<(string ConversationId, string LastMessage, DateTime LastAt)>>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task GetConversations_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetConversations();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
