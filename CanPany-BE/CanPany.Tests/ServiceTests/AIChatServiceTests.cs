using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class AIChatServiceTests
{
    private readonly Mock<IGeminiService> _geminiServiceMock = new();
    private readonly Mock<IConversationRepository> _conversationRepositoryMock = new();
    private readonly Mock<IMessageRepository> _messageRepositoryMock = new();
    private readonly Mock<ILogger<AIChatService>> _loggerMock = new();
    private readonly AIChatService _service;

    public AIChatServiceTests()
    {
        _service = new AIChatService(
            _geminiServiceMock.Object,
            _conversationRepositoryMock.Object,
            _messageRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ChatAsync_ShouldReturnResponse_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var message = "What skills do I need for a software engineer role?";
        var aiResponse = "You need C#, .NET, etc.";

        _conversationRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Conversation>()))
            .ReturnsAsync(new Conversation { Id = "conv1" });
            
        _geminiServiceMock.Setup(x => x.GenerateChatResponseAsync(It.IsAny<string>(), message))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.ChatAsync(userId, message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(aiResponse, result);
    }

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenUserIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChatAsync("", "Hello"));
    }

    [Fact]
    public async Task ChatAsync_ShouldThrow_WhenMessageIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChatAsync("user123", ""));
    }

    [Fact]
    public async Task GetConversationsAsync_ShouldReturnEmptyList_WhenNoConversations()
    {
        // Arrange
        var userId = "user123";

        // Act
        var result = await _service.GetConversationsAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetConversationsAsync_ShouldThrow_WhenUserIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetConversationsAsync(""));
    }
}
