using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
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
    /// UC-COM-05: Get all notifications for current user with optional filters
    /// </summary>
    /// <param name="isRead">Filter by read status (null = all, true = read only, false = unread only)</param>
    /// <param name="type">Filter by notification type (e.g., "ProposalAccepted", "NewMessage", "JobMatch", "PaymentConfirmation")</param>
    /// <param name="fromDate">Filter notifications created after this date</param>
    /// <param name="toDate">Filter notifications created before this date</param>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var filter = new NotificationFilterDto
            {
                IsRead = isRead,
                Type = type,
                FromDate = fromDate,
                ToDate = toDate
            };

            var notifications = await _notificationService.GetFilteredNotificationsAsync(userId, filter);
            return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.CreateSuccess(notifications));
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


