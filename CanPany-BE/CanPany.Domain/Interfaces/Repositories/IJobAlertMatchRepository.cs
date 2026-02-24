using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

public interface IJobAlertMatchRepository
{
    Task<bool> MatchExistsAsync(string jobAlertId, string jobId);
    Task<IEnumerable<JobAlertMatch>> GetByJobAlertIdAsync(string jobAlertId);
    Task<IEnumerable<JobAlertMatch>> GetByUserIdAsync(string userId, DateTime? fromDate = null);
    Task<JobAlertMatch> AddAsync(JobAlertMatch match);
    Task<int> GetMatchCountForAlertAsync(string jobAlertId);
}