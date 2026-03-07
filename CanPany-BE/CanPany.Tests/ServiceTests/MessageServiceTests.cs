using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class MessageServiceTests
{
    private readonly Mock<IMessageRepository> _repositoryMock = new();
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<ILogger<MessageService>> _loggerMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly MessageService _service;

    public MessageServiceTests()
    {
        _configurationMock.Setup(x => x["Encryption:Key"])
            .Returns("TestEncryptionKey-MustBeAtLeast32Characters!");

        // Setup encryption mock to encrypt/decrypt transparently for existing tests
        _encryptionServiceMock.Setup(x => x.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((text, key) => $"ENC:{text}");
        _encryptionServiceMock.Setup(x => x.Decrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((text, key) => text.StartsWith("ENC:") ? text.Substring(4) : text);

        _service = new MessageService(
            _repositoryMock.Object,
            _encryptionServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
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
            Text = "ENC:Hello"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(messageId))
            .ReturnsAsync(message);

        // Act
        var result = await _service.GetByIdAsync(messageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(messageId, result.Id);
        Assert.Equal("Hello", result.Text); // Should be decrypted
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetByConversationIdAsync_ShouldReturnDecryptedMessages()
    {
        // Arrange
        var conversationId = "conv123";
        var messages = new List<Message>
        {
            new Message { Id = "msg1", ConversationId = conversationId, SenderId = "user123", Text = "ENC:Hello" },
            new Message { Id = "msg2", ConversationId = conversationId, SenderId = "user456", Text = "ENC:Hi" }
        };
        
        _repositoryMock.Setup(x => x.GetByConversationIdAsync(conversationId, 1, 50))
            .ReturnsAsync(messages);

        // Act
        var result = await _service.GetByConversationIdAsync(conversationId);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Hello", resultList[0].Text);
        Assert.Equal("Hi", resultList[1].Text);
    }

    [Fact]
    public async Task SendAsync_ShouldEncryptMessage_BeforeSaving()
    {
        // Arrange
        var conversationId = "conv123";
        var senderId = "user123";
        var text = "Hello";
        string? capturedText = null;

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Message>()))
            .Callback<Message>(m => capturedText = m.Text) // Capture text at call time
            .ReturnsAsync((Message m) => m);

        // Act
        var result = await _service.SendAsync(conversationId, senderId, text);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello", result.Text); // Returned text should be decrypted

        // Verify that Encrypt was called
        _encryptionServiceMock.Verify(
            x => x.Encrypt(text, It.IsAny<string>()), 
            Times.Once);

        // Verify that the repository received encrypted text (captured at call time)
        Assert.Equal("ENC:Hello", capturedText);
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
