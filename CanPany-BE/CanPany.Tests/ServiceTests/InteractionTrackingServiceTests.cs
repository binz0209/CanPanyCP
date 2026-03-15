using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class InteractionTrackingServiceTests
{
    private readonly Mock<IUserJobInteractionRepository> _interactionRepoMock = new();
    private readonly Mock<ILogger<InteractionTrackingService>> _loggerMock = new();
    private readonly InteractionTrackingService _service;

    public InteractionTrackingServiceTests()
    {
        _service = new InteractionTrackingService(_interactionRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task TrackInteractionAsync_ShouldSaveNewInteraction()
    {
        // Arrange
        _interactionRepoMock.Setup(x => x.GetByUserJobAndTypeAsync("user1", "job1", InteractionType.View))
            .ReturnsAsync((UserJobInteraction?)null);
        _interactionRepoMock.Setup(x => x.AddAsync(It.IsAny<UserJobInteraction>()))
            .ReturnsAsync((UserJobInteraction i) => i);

        // Act
        var result = await _service.TrackInteractionAsync("user1", "job1", InteractionType.View);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result!.UserId);
        Assert.Equal("job1", result.JobId);
        Assert.Equal(InteractionType.View, result.Type);
        Assert.Equal(1.0, result.Score);
        _interactionRepoMock.Verify(x => x.AddAsync(It.IsAny<UserJobInteraction>()), Times.Once);
    }

    [Fact]
    public async Task TrackInteractionAsync_ShouldReturnExisting_WhenDuplicate()
    {
        // Arrange
        var existing = new UserJobInteraction
        {
            Id = "int1", UserId = "user1", JobId = "job1",
            Type = InteractionType.Click, Score = 2.0
        };
        _interactionRepoMock.Setup(x => x.GetByUserJobAndTypeAsync("user1", "job1", InteractionType.Click))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.TrackInteractionAsync("user1", "job1", InteractionType.Click);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("int1", result!.Id);
        _interactionRepoMock.Verify(x => x.AddAsync(It.IsAny<UserJobInteraction>()), Times.Never);
    }

    [Fact]
    public async Task TrackInteractionAsync_ShouldThrow_WhenUserIdEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.TrackInteractionAsync("", "job1", InteractionType.View));
    }

    [Fact]
    public async Task TrackInteractionAsync_ShouldThrow_WhenJobIdEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.TrackInteractionAsync("user1", "", InteractionType.View));
    }

    [Theory]
    [InlineData(InteractionType.View, 1.0)]
    [InlineData(InteractionType.Click, 2.0)]
    [InlineData(InteractionType.Bookmark, 3.0)]
    [InlineData(InteractionType.Apply, 5.0)]
    public void GetImplicitScore_ShouldReturnCorrectMapping(InteractionType type, double expected)
    {
        var score = InteractionTrackingService.GetImplicitScore(type);
        Assert.Equal(expected, score);
    }

    [Fact]
    public async Task GetUserInteractionCountAsync_ShouldReturnCount()
    {
        // Arrange
        _interactionRepoMock.Setup(x => x.GetCountByUserIdAsync("user1")).ReturnsAsync(42);

        // Act
        var count = await _service.GetUserInteractionCountAsync("user1");

        // Assert
        Assert.Equal(42, count);
    }

    [Fact]
    public async Task GetUserInteractionCountAsync_ShouldReturnZero_WhenEmptyUserId()
    {
        var count = await _service.GetUserInteractionCountAsync("");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetUserInteractionsAsync_ShouldReturnEmpty_WhenEmptyUserId()
    {
        var result = await _service.GetUserInteractionsAsync("");
        Assert.Empty(result);
    }
}
