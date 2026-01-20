using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Job service interface
/// </summary>
public interface IJobService
{
    Task<Job?> GetByIdAsync(string id);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Job>> GetByStatusAsync(string status);
    Task<IEnumerable<Job>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget);
    Task<Job> CreateAsync(Job job);
    Task<bool> UpdateAsync(string id, Job job);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


