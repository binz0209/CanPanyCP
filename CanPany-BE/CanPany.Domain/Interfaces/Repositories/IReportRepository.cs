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
    Task<Report> AddAsync(Report report);
    Task UpdateAsync(Report report);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

