using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for JobBookmark entity
/// </summary>
public interface IJobBookmarkRepository
{
    Task<JobBookmark?> GetByIdAsync(string id);
    Task<JobBookmark?> GetByUserAndJobAsync(string userId, string jobId);
    Task<IEnumerable<JobBookmark>> GetByUserIdAsync(string userId);
    Task<JobBookmark> AddAsync(JobBookmark bookmark);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string userId, string jobId);
}


