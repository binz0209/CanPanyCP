using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

public interface IJobAlertRepository
{
    Task<JobAlert?> GetByIdAsync(string id);
    Task<IEnumerable<JobAlert>> GetByUserIdAsync(string userId);
    Task<IEnumerable<JobAlert>> GetActiveAlertsAsync();
    Task<IEnumerable<JobAlert>> GetActiveAlertsByFrequencyAsync(string frequency);
    Task<JobAlert> AddAsync(JobAlert alert);
    Task UpdateAsync(JobAlert alert);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

