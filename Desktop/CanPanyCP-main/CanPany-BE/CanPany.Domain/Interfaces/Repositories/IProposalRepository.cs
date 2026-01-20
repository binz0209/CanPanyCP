using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Proposal entity
/// </summary>
public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(string id);
    Task<IEnumerable<Proposal>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<Proposal>> GetByFreelancerIdAsync(string freelancerId);
    Task<Proposal?> GetByProjectAndFreelancerAsync(string projectId, string freelancerId);
    Task<Proposal> AddAsync(Proposal proposal);
    Task UpdateAsync(Proposal proposal);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


