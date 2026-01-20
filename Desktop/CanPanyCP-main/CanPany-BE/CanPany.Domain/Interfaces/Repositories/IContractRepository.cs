using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Contract entity
/// </summary>
public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(string id);
    Task<IEnumerable<Contract>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<Contract>> GetByClientIdAsync(string clientId);
    Task<IEnumerable<Contract>> GetByFreelancerIdAsync(string freelancerId);
    Task<Contract> AddAsync(Contract contract);
    Task UpdateAsync(Contract contract);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


