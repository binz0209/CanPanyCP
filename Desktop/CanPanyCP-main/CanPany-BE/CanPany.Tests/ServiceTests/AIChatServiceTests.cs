using CanPany.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class AIChatServiceTests
{
    private readonly Mock<ILogger<AIChatService>> _loggerMock = new();
    private readonly AIChatService _service;

    public AIChatServiceTests()
    {
        _service = new AIChatService(_loggerMock.Object);
    }

    [Fact]
    public async Task ChatAsync_ShouldReturnResponse_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var message = "What skills do I need for a software engineer role?";

        // Act
        var result = await _service.ChatAsync(userId, message);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
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
