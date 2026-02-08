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

    public async Task<IEnumerable<Report>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByStatusAsync(string status)
    {
        return await _collection.Find(r => r.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _collection.Find(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate).ToListAsync();
    }

    public async Task<IEnumerable<Report>> GetWithFiltersAsync(string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var builder = Builders<Report>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(status))
        {
            filter &= builder.Eq(r => r.Status, status);
        }

        if (fromDate.HasValue)
        {
            filter &= builder.Gte(r => r.CreatedAt, fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filter &= builder.Lte(r => r.CreatedAt, toDate.Value);
        }

        return await _collection.Find(filter).ToListAsync();
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
}

