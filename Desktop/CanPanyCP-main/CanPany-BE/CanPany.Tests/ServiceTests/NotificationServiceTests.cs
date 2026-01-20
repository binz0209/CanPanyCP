using CanPany.Application.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repositoryMock = new();
    private readonly Mock<ILogger<NotificationService>> _loggerMock = new();
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _service = new NotificationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotification_WhenExists()
    {
        // Arrange
        var notificationId = "notif123";
        var notification = new Notification
        {
            Id = notificationId,
            UserId = "user123",
            Title = "New Job Match",
            Message = "A new job matches your profile"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.GetByIdAsync(notificationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notificationId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNotifications_WhenExist()
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<Notification>
        {
            new Notification { Id = "notif1", UserId = userId, Title = "Title 1", IsRead = false },
            new Notification { Id = "notif2", UserId = userId, Title = "Title 2", IsRead = true }
        };
        
        _repositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUnreadByUserIdAsync_ShouldReturnUnreadNotifications_WhenExist()
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<Notification>
        {
            new Notification { Id = "notif1", UserId = userId, IsRead = false },
            new Notification { Id = "notif2", UserId = userId, IsRead = false }
        };
        
        _repositoryMock.Setup(x => x.GetUnreadByUserIdAsync(userId))
            .ReturnsAsync(notifications);

        // Act
        var result = await _service.GetUnreadByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, n => Assert.False(n.IsRead));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotification_WhenValid()
    {
        // Arrange
        var notification = new Notification
        {
            UserId = "user123",
            Title = "New Job Match",
            Message = "A new job matches your profile"
        };
        var savedNotification = new Notification
        {
            Id = "notif123",
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = false
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync(savedNotification);

        // Act
        var result = await _service.CreateAsync(notification);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsRead);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNotificationIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldReturnTrue_WhenNotificationExists()
    {
        // Arrange
        var notificationId = "notif123";
        var notification = new Notification
        {
            Id = notificationId,
            IsRead = false
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.MarkAsReadAsync(notificationId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldThrow_WhenNotificationNotExists()
    {
        // Arrange
        var notificationId = "nonexistent";
        _repositoryMock.Setup(x => x.MarkAsReadAsync(notificationId))
            .ThrowsAsync(new InvalidOperationException("Notification not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MarkAsReadAsync(notificationId));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var userId = "user123";
        _repositoryMock.Setup(x => x.MarkAllAsReadAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.MarkAllAsReadAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnCount_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var count = 5;
        _repositoryMock.Setup(x => x.GetUnreadCountAsync(userId))
            .ReturnsAsync(count);

        // Act
        var result = await _service.GetUnreadCountAsync(userId);

        // Assert
        Assert.Equal(count, result);
    }
}
