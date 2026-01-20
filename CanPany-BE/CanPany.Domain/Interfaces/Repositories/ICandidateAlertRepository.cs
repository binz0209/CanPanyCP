using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for CandidateAlert entity
/// </summary>
public interface ICandidateAlertRepository
{
    Task<CandidateAlert?> GetByIdAsync(string id);
    Task<IEnumerable<CandidateAlert>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<CandidateAlert>> GetActiveAlertsAsync(string companyId);
    Task<CandidateAlert> AddAsync(CandidateAlert candidateAlert);
    Task UpdateAsync(CandidateAlert candidateAlert);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

