using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Repositories;

public class JobAlertMatchRepository : IJobAlertMatchRepository
{
    private readonly IMongoCollection<JobAlertMatch> _collection;
    private readonly ILogger<JobAlertMatchRepository>? _logger;

    public JobAlertMatchRepository(MongoDbContext context, ILogger<JobAlertMatchRepository>? logger = null)
    {
        _collection = context.JobAlertMatches;
        _logger = logger;
    }

    public async Task<bool> MatchExistsAsync(string jobAlertId, string jobId)
    {
        var count = await _collection.CountDocumentsAsync(m => 
            m.JobAlertId == jobAlertId && m.JobId == jobId);
        return count > 0;
    }

    public async Task<IEnumerable<JobAlertMatch>> GetByJobAlertIdAsync(string jobAlertId)
    {
        return await _collection.Find(m => m.JobAlertId == jobAlertId)
            .SortByDescending(m => m.MatchedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<JobAlertMatch>> GetByUserIdAsync(string userId, DateTime? fromDate = null)
    {
        var filterBuilder = Builders<JobAlertMatch>.Filter;
        var filter = filterBuilder.Eq(m => m.UserId, userId);

        if (fromDate.HasValue)
        {
            filter &= filterBuilder.Gte(m => m.MatchedAt, fromDate.Value);
        }

        return await _collection.Find(filter)
            .SortByDescending(m => m.MatchedAt)
            .ToListAsync();
    }

    public async Task<JobAlertMatch> AddAsync(JobAlertMatch match)
    {
        await _collection.InsertOneAsync(match);
        return match;
    }

    public async Task<int> GetMatchCountForAlertAsync(string jobAlertId)
    {
        var count = await _collection.CountDocumentsAsync(m => m.JobAlertId == jobAlertId);
        return (int)count;
    }
}