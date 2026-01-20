using CanPany.Application.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _repositoryMock = new();
    private readonly Mock<ILogger<MessageService>> _loggerMock = new();
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _service = new MessageService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMessage_WhenExists()
    {
        // Arrange
        var messageId = "msg123";
        var message = new Message
        {
            Id = messageId,
            SenderId = "user123",
            ReceiverId = "user456",
            Text = "Hello"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        var result = await _service.GetByIdAsync(messageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(messageId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetByConversationKeyAsync_ShouldReturnMessages_WhenExist()
    {
        // Arrange
        var conversationKey = "user123_user456";
        var messages = new List<Message>
        {
            new Message { Id = "msg1", SenderId = "user123", ReceiverId = "user456", Text = "Hello" },
            new Message { Id = "msg2", SenderId = "user456", ReceiverId = "user123", Text = "Hi" }
        };
        
        _repositoryMock.Setup(x => x.GetByConversationKeyAsync(conversationKey))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetByConversationKeyAsync(conversationKey);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnAllMessages_WhenExist()
    {
        // Arrange
        var userId = "user123";
        var sentMessages = new List<Message>
        {
            new Message { Id = "msg1", SenderId = userId, ReceiverId = "user456", Text = "Hello", CreatedAt = DateTime.UtcNow }
        };
        var receivedMessages = new List<Message>
        {
            new Message { Id = "msg2", SenderId = "user456", ReceiverId = userId, Text = "Hi", CreatedAt = DateTime.UtcNow.AddHours(1) }
        };
        
        _repositoryMock.Setup(x => x.GetBySenderIdAsync(userId))
            .ReturnsAsync(sentMessages);
        _repositoryMock.Setup(x => x.GetByReceiverIdAsync(userId))
            .ReturnsAsync(receivedMessages);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SendAsync_ShouldReturnMessage_WhenValid()
    {
        // Arrange
        var message = new Message
        {
            SenderId = "user123",
            ReceiverId = "user456",
            Text = "Hello",
            CreatedAt = DateTime.UtcNow
        };
        var savedMessage = new Message
        {
            Id = "msg123",
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Text = message.Text,
            CreatedAt = message.CreatedAt
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _service.SendAsync(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("msg123", result.Id);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldReturnTrue_WhenMessageExists()
    {
        // Arrange
        var messageId = "msg123";
        var message = new Message
        {
            Id = messageId,
            IsRead = false
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(messageId))
            .ReturnsAsync(message);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Message>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.MarkAsReadAsync(messageId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenMessageNotExists()
    {
        // Arrange
        var messageId = "nonexistent";
        _repositoryMock.Setup(x => x.MarkAsReadAsync(messageId))
            .ThrowsAsync(new InvalidOperationException("Message not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MarkAsReadAsync(messageId));
    }
}
