using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IMongoCollection<Notification> _collection;

    public NotificationRepository(MongoDbContext context)
    {
        _collection = context.Notifications;
    }

    public async Task<Notification?> GetByIdAsync(string id)
    {
        return await _collection.Find(n => n.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(n => n.UserId == userId)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId)
    {
        return await _collection.Find(n => n.UserId == userId && !n.IsRead)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetFilteredByUserIdAsync(
        string userId, 
        bool? isRead, 
        string? type, 
        DateTime? fromDate, 
        DateTime? toDate)
    {
        var filterBuilder = Builders<Notification>.Filter;
        var filters = new List<FilterDefinition<Notification>>
        {
            filterBuilder.Eq(n => n.UserId, userId)
        };

        // Apply isRead filter if specified
        if (isRead.HasValue)
        {
            filters.Add(filterBuilder.Eq(n => n.IsRead, isRead.Value));
        }

        // Apply type filter if specified
        if (!string.IsNullOrWhiteSpace(type))
        {
            filters.Add(filterBuilder.Eq(n => n.Type, type));
        }

        // Apply date range filters if specified
        if (fromDate.HasValue)
        {
            filters.Add(filterBuilder.Gte(n => n.CreatedAt, fromDate.Value));
        }

        if (toDate.HasValue)
        {
            filters.Add(filterBuilder.Lte(n => n.CreatedAt, toDate.Value));
        }

        var combinedFilter = filterBuilder.And(filters);

        return await _collection.Find(combinedFilter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        await _collection.InsertOneAsync(notification);
        return notification;
    }

    public async Task UpdateAsync(Notification notification)
    {
        notification.MarkAsUpdated();
        await _collection.ReplaceOneAsync(n => n.Id == notification.Id, notification);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(n => n.Id == id);
    }

    public async Task MarkAsReadAsync(string notificationId)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _collection.UpdateOneAsync(n => n.Id == notificationId, update);
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _collection.UpdateManyAsync(n => n.UserId == userId && !n.IsRead, update);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var count = await _collection.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
        return (int)count;
    }
}


