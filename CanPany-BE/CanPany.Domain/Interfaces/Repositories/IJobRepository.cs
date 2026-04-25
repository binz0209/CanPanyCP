using CanPany.Domain.Entities;
using CanPany.Domain.Models;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Job entity
/// </summary>
public interface IJobRepository
{
    Task<Job?> GetByIdAsync(string id);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Job>> GetByStatusAsync(string status);
    Task<IEnumerable<Job>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget);
    Task<(long TotalCount, IEnumerable<Job> Jobs)> SearchPagedAsync(JobSearchParameters parameters);
    Task<IEnumerable<Job>> GetJobsCreatedAfterAsync(DateTime date);
    Task<Job> AddAsync(Job job);
    Task UpdateAsync(Job job);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

