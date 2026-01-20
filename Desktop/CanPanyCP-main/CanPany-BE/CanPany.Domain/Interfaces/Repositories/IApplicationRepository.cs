using CanPany.Domain.Entities;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Application entity
/// </summary>
public interface IApplicationRepository
{
    Task<Application?> GetByIdAsync(string id);
    Task<IEnumerable<Application>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<Application>> GetByCandidateIdAsync(string candidateId);
    Task<Application?> GetByJobAndCandidateAsync(string jobId, string candidateId);
    Task<IEnumerable<Application>> GetAllAsync();
    Task<Application> AddAsync(Application application);
    Task UpdateAsync(Application application);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<bool> HasAppliedAsync(string jobId, string candidateId);
}
