using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Contract entity
/// </summary>
public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(string id);
    Task<IEnumerable<Contract>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<Contract>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Contract>> GetByCandidateIdAsync(string candidateId);
    Task<Contract?> GetByApplicationIdAsync(string applicationId);
    Task<Contract> AddAsync(Contract contract);
    Task UpdateAsync(Contract contract);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


