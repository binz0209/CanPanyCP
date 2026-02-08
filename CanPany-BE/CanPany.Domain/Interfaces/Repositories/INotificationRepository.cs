using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Notification entity
/// </summary>
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(string id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId);
    Task<IEnumerable<Notification>> GetFilteredByUserIdAsync(string userId, bool? isRead, string? type, DateTime? fromDate, DateTime? toDate);
    Task<Notification> AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(string id);
    Task MarkAsReadAsync(string notificationId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}


