using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class JobAlertRepository : IJobAlertRepository
{
    private readonly IMongoCollection<JobAlert> _collection;

    public JobAlertRepository(MongoDbContext context)
    {
        _collection = context.JobAlerts;
    }

    public async Task<JobAlert?> GetByIdAsync(string id)
    {
        return await _collection.Find(ja => ja.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(ja => ja.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<JobAlert>> GetActiveAlertsAsync(string userId)
    {
        return await _collection.Find(ja => ja.UserId == userId && ja.IsActive == true).ToListAsync();
    }

    public async Task<JobAlert> AddAsync(JobAlert jobAlert)
    {
        await _collection.InsertOneAsync(jobAlert);
        return jobAlert;
    }

    public async Task UpdateAsync(JobAlert jobAlert)
    {
        jobAlert.MarkAsUpdated();
        await _collection.ReplaceOneAsync(ja => ja.Id == jobAlert.Id, jobAlert);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(ja => ja.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(ja => ja.Id == id);
        return count > 0;
    }
}

