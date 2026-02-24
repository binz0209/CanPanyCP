using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Repositories;

public class JobAlertRepository : IJobAlertRepository
{
    private readonly IMongoCollection<JobAlert> _collection;
    private readonly ILogger<JobAlertRepository>? _logger;

    public JobAlertRepository(MongoDbContext context, ILogger<JobAlertRepository>? logger = null)
    {
        _collection = context.JobAlerts;
        _logger = logger;
    }

    public async Task<JobAlert?> GetByIdAsync(string id)
    {
        return await _collection.Find(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(a => a.UserId == userId)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetActiveAlertsAsync()
    {
        return await _collection.Find(a => a.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetActiveAlertsByFrequencyAsync(string frequency)
    {
        return await _collection.Find(a => a.IsActive && a.Frequency == frequency)
            .ToListAsync();
    }

    public async Task<JobAlert> AddAsync(JobAlert alert)
    {
        await _collection.InsertOneAsync(alert);
        return alert;
    }

    public async Task UpdateAsync(JobAlert alert)
    {
        alert.MarkAsUpdated();
        await _collection.ReplaceOneAsync(a => a.Id == alert.Id, alert);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(a => a.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(a => a.Id == id);
        return count > 0;
    }
}

