using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Notifications controller - UC-COM-05, UC-COM-06
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// UC-COM-05: View Notification List
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<Notification>>.CreateSuccess(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, ApiResponse.CreateError("Failed to get notifications", "GetNotificationsFailed"));
        }
    }

    /// <summary>
    /// Get unread notifications
    /// </summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadByUserIdAsync(userId);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<object>.CreateSuccess(new { notifications, unreadCount = count }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications");
            return StatusCode(500, ApiResponse.CreateError("Failed to get unread notifications", "GetUnreadNotificationsFailed"));
        }
    }

    /// <summary>
    /// UC-COM-06: Mark Notification as Read
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        try
        {
            var succeeded = await _notificationService.MarkAsReadAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Notification not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Notification marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, ApiResponse.CreateError("Failed to mark notification as read", "MarkAsReadFailed"));
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse.CreateSuccess("All notifications marked as read"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, ApiResponse.CreateError("Failed to mark all as read", "MarkAllAsReadFailed"));
        }
    }
}


