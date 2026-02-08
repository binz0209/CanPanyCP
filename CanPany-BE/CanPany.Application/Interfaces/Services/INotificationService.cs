using CanPany.Domain.Entities;
using CanPany.Application.DTOs;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Notification service interface
/// </summary>
public interface INotificationService
{
    Task<Notification?> GetByIdAsync(string id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
    Task<IEnumerable<NotificationResponseDto>> GetFilteredNotificationsAsync(string userId, NotificationFilterDto filter);
    Task<Notification> CreateAsync(Notification notification);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}


