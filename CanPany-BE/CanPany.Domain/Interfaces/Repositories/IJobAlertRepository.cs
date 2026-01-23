using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for JobAlert entity
/// </summary>
public interface IJobAlertRepository
{
    Task<JobAlert?> GetByIdAsync(string id);
    Task<IEnumerable<JobAlert>> GetAllAsync();
    Task<IEnumerable<JobAlert>> GetByUserIdAsync(string userId);
    Task<IEnumerable<JobAlert>> GetActiveAlertsAsync(string userId);
    Task<JobAlert> AddAsync(JobAlert jobAlert);
    Task UpdateAsync(JobAlert jobAlert);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

