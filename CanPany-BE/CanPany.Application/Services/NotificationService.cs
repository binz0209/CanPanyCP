using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Notification service implementation
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        ILogger<NotificationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Notification ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification by ID: {NotificationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetUnreadByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications by user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetFilteredNotificationsAsync(
        string userId, 
        NotificationFilterDto filter)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var notifications = await _repo.GetFilteredByUserIdAsync(
                userId,
                filter.IsRead,
                filter.Type,
                filter.FromDate,
                filter.ToDate);

            // Map to response DTOs
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title ?? string.Empty,
                Content = n.Message ?? string.Empty,
                Timestamp = n.CreatedAt,
                IsRead = n.IsRead
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered notifications for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        try
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            return await _repo.AddAsync(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("Notification ID cannot be null or empty", nameof(notificationId));

            await _repo.MarkAsReadAsync(notificationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            throw;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            await _repo.MarkAllAsReadAsync(userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetUnreadCountAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count: {UserId}", userId);
            throw;
        }
    }
}


