using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Report entity
/// </summary>
public interface IReportRepository
{
    Task<Report?> GetByIdAsync(string id);
    Task<IEnumerable<Report>> GetByReporterIdAsync(string reporterId);
    Task<IEnumerable<Report>> GetByReportedUserIdAsync(string reportedUserId);
    Task<IEnumerable<Report>> GetByReportedCompanyIdAsync(string reportedCompanyId);
    Task<IEnumerable<Report>> GetPendingReportsAsync();
    Task<IEnumerable<Report>> GetResolvedReportsAsync();
    Task<IEnumerable<Report>> GetAllAsync();
    Task<IEnumerable<Report>> GetByStatusAsync(string status);
    Task<IEnumerable<Report>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<Report>> GetWithFiltersAsync(string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Report> AddAsync(Report report);
    Task UpdateAsync(Report report);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


