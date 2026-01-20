using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CanPany.Tests.ControllerTests;

public class MessagesControllerTests
{
    private readonly Mock<IMessageService> _messageServiceMock = new();
    private readonly Mock<ILogger<MessagesController>> _loggerMock = new();
    private readonly MessagesController _controller;

    public MessagesControllerTests()
    {
        _controller = new MessagesController(
            _messageServiceMock.Object,
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
    public async Task SendMessage_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var request = new SendMessageRequest("receiver456", "Hello, how are you?", "project789");
        var message = new Message
        {
            Id = "msg123",
            SenderId = userId,
            ReceiverId = request.ReceiverId,
            ProjectId = request.ProjectId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };
        
        _messageServiceMock.Setup(x => x.SendAsync(It.IsAny<Message>()))
            .ReturnsAsync(message);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Message>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(message, response.Data);
    }

    [Fact]
    public async Task SendMessage_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };
        var request = new SendMessageRequest("receiver456", "Hello", null);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetMessages_ShouldReturnSuccess_WhenConversationKeyProvided()
    {
        // Arrange
        var conversationKey = "user123_receiver456";
        var messages = new List<Message>
        {
            new Message { Id = "msg1", SenderId = "user123", ReceiverId = "receiver456", Text = "Hello" },
            new Message { Id = "msg2", SenderId = "receiver456", ReceiverId = "user123", Text = "Hi there" }
        };
        
        _messageServiceMock.Setup(x => x.GetByConversationKeyAsync(conversationKey))
            .ReturnsAsync(messages);

        // Act
        var result = await _controller.GetMessages(conversationKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Message>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetMessages_ShouldReturnSuccess_WhenNoConversationKey()
    {
        // Arrange
        var userId = "user123";
        var messages = new List<Message>
        {
            new Message { Id = "msg1", SenderId = userId, ReceiverId = "receiver456", Text = "Hello" }
        };
        
        _messageServiceMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(messages);

        // Act
        var result = await _controller.GetMessages(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Message>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetConversations_ShouldReturnSuccess_WhenConversationsExist()
    {
        // Arrange
        var userId = "user123";
        var conversations = new List<(string ConversationKey, string PartnerId, string LastMessage, DateTime LastAt, int UnreadCount)>
        {
            ("conv1", "partner1", "Last message", DateTime.UtcNow, 2),
            ("conv2", "partner2", "Another message", DateTime.UtcNow.AddHours(-1), 0)
        };
        
        _messageServiceMock.Setup(x => x.GetConversationsForUserAsync(userId))
            .ReturnsAsync(conversations);

        // Act
        var result = await _controller.GetConversations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<(string ConversationKey, string PartnerId, string LastMessage, DateTime LastAt, int UnreadCount)>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnSuccess_WhenMessageExists()
    {
        // Arrange
        var messageId = "msg123";
        _messageServiceMock.Setup(x => x.MarkAsReadAsync(messageId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsRead(messageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenMessageNotExists()
    {
        // Arrange
        var messageId = "nonexistent";
        _messageServiceMock.Setup(x => x.MarkAsReadAsync(messageId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkAsRead(messageId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task MarkConversationAsRead_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var conversationKey = "user123_receiver456";
        _messageServiceMock.Setup(x => x.MarkConversationAsReadAsync(conversationKey, userId))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.MarkConversationAsRead(conversationKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
