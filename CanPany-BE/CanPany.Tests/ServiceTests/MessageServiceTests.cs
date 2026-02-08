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
            ConversationId = "conv123",
            SenderId = "user123",
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
    public async Task GetByConversationIdAsync_ShouldReturnMessages_WhenExist()
    {
        // Arrange
        var conversationId = "conv123";
        var messages = new List<Message>
        {
            new Message { Id = "msg1", ConversationId = conversationId, SenderId = "user123", Text = "Hello" },
            new Message { Id = "msg2", ConversationId = conversationId, SenderId = "user456", Text = "Hi" }
        };
        
        _repositoryMock.Setup(x => x.GetByConversationIdAsync(conversationId, 1, 50))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetByConversationIdAsync(conversationId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SendAsync_ShouldReturnMessage_WhenValid()
    {
        // Arrange
        var conversationId = "conv123";
        var senderId = "user123";
        var text = "Hello";
        var savedMessage = new Message
        {
            Id = "msg123",
            ConversationId = conversationId,
            SenderId = senderId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        // Act
        var result = await _service.SendAsync(conversationId, senderId, text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("msg123", result.Id);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldReturnTrue_WhenMessageExists()
    {
        // Arrange
        var messageId = "msg123";
        
        _repositoryMock.Setup(x => x.MarkAsReadAsync(messageId))
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
