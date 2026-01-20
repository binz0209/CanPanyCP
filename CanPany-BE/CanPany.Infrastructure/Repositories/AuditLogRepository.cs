using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public AuditLogRepository(MongoDbContext context)
    {
        _collection = context.AuditLogs;
    }

    public async Task<AuditLog?> GetByIdAsync(string id)
    {
        return await _collection.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(a => a.CreatedAt)
            .Limit(1000)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100)
    {
        return await _collection.Find(a => a.UserId == userId)
            .SortByDescending(a => a.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, string? entityId = null, int limit = 100)
    {
        var filterBuilder = Builders<AuditLog>.Filter;
        var filter = filterBuilder.Eq(a => a.EntityType, entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(a => a.EntityId, entityId));
        }

        return await _collection.Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000)
    {
        var filter = Builders<AuditLog>.Filter.And(
            Builders<AuditLog>.Filter.Gte(a => a.CreatedAt, startDate),
            Builders<AuditLog>.Filter.Lte(a => a.CreatedAt, endDate)
        );

        return await _collection.Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<AuditLog> AddAsync(AuditLog auditLog)
    {
        await _collection.InsertOneAsync(auditLog);
        return auditLog;
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(a => a.Id == id);
    }
}


