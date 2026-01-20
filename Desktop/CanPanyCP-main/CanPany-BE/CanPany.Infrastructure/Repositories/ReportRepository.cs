using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Infrastructure.Data;
using MongoDB.Driver;

namespace CanPany.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IMongoCollection<Report> _collection;

    public ReportRepository(MongoDbContext context)
    {
        _collection = context.Reports;
    }

    public async Task<Report?> GetByIdAsync(string id)
    {
        return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Report>> GetByReporterIdAsync(string reporterId)
    {
        return await _collection.Find(r => r.ReporterId == reporterId).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByReportedUserIdAsync(string reportedUserId)
    {
        return await _collection.Find(r => r.ReportedUserId == reportedUserId).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByReportedCompanyIdAsync(string reportedCompanyId)
    {
        return await _collection.Find(r => r.ReportedCompanyId == reportedCompanyId).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetPendingReportsAsync()
    {
        return await _collection.Find(r => r.Status == "Pending").ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetResolvedReportsAsync()
    {
        return await _collection.Find(r => r.Status == "Resolved").ToListAsync();
    }

    public async Task<Report> AddAsync(Report report)
    {
        await _collection.InsertOneAsync(report);
        return report;
    }

    public async Task UpdateAsync(Report report)
    {
        report.MarkAsUpdated();
        await _collection.ReplaceOneAsync(r => r.Id == report.Id, report);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(r => r.Id == id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _collection.CountDocumentsAsync(r => r.Id == id);
        return count > 0;
    }

    public async Task<IEnumerable<Report>> GetAllAsync()
    {
        return await _collection.Find(_ => true)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByStatusAsync(string status)
    {
        return await _collection.Find(r => r.Status == status)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _collection.Find(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetWithFiltersAsync(string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var filterBuilder = Builders<Report>.Filter;
        var filters = new List<FilterDefinition<Report>>();

        // Always include all reports as base
        filters.Add(filterBuilder.Empty);

        // Add status filter if provided
        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add(filterBuilder.Eq(r => r.Status, status));
        }

        // Add date range filter if provided
        if (fromDate.HasValue)
        {
            filters.Add(filterBuilder.Gte(r => r.CreatedAt, fromDate.Value));
        }

        if (toDate.HasValue)
        {
            filters.Add(filterBuilder.Lte(r => r.CreatedAt, toDate.Value));
        }

        var combinedFilter = filterBuilder.And(filters);

        return await _collection.Find(combinedFilter)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}

