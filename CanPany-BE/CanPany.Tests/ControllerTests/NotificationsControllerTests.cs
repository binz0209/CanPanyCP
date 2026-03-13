using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CanPany.Tests.ControllerTests;

public class NotificationsControllerTests
{
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ILogger<NotificationsController>> _loggerMock = new();
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _controller = new NotificationsController(
            _notificationServiceMock.Object,
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
    public async Task GetNotifications_ShouldReturnSuccess_WhenNotificationsExist()
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<NotificationResponseDto>
        {
            new NotificationResponseDto
            { 
                Id = "notif1", 
                Title = "New Job Match", 
                Content = "A new job matches your profile",
                IsRead = false,
                Timestamp = DateTime.UtcNow
            },
            new NotificationResponseDto
            { 
                Id = "notif2", 
                Title = "Application Update", 
                Content = "Your application has been reviewed",
                IsRead = true,
                Timestamp = DateTime.UtcNow.AddHours(-1)
            }
        };
        
        _notificationServiceMock.Setup(x => x.GetFilteredNotificationsAsync(userId, It.IsAny<NotificationFilterDto>()))
            .ReturnsAsync(notifications);

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<NotificationResponseDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetNotifications();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetUnreadNotifications_ShouldReturnSuccess_WhenUnreadNotificationsExist()
    {
        // Arrange
        var userId = "user123";
        var notifications = new List<Notification>
        {
            new Notification 
            { 
                Id = "notif1", 
                UserId = userId, 
                Title = "New Job Match", 
                IsRead = false
            }
        };
        var unreadCount = 1;
        
        _notificationServiceMock.Setup(x => x.GetUnreadByUserIdAsync(userId))
            .ReturnsAsync(notifications);
        _notificationServiceMock.Setup(x => x.GetUnreadCountAsync(userId))
            .ReturnsAsync(unreadCount);

        // Act
        var result = await _controller.GetUnreadNotifications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnSuccess_WhenNotificationExists()
    {
        // Arrange
        var notificationId = "notif123";
        _notificationServiceMock.Setup(x => x.MarkAsReadAsync(notificationId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationNotExists()
    {
        // Arrange
        var notificationId = "nonexistent";
        _notificationServiceMock.Setup(x => x.MarkAsReadAsync(notificationId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkAsRead(notificationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        _notificationServiceMock.Setup(x => x.MarkAllAsReadAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkAllAsRead();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
