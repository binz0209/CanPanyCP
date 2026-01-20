using CanPany.Domain.Entities;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Application service interface
/// </summary>
public interface IApplicationService
{
    Task<DomainApplication?> GetByIdAsync(string id);
    Task<IEnumerable<DomainApplication>> GetByJobIdAsync(string jobId);
    Task<IEnumerable<DomainApplication>> GetByCandidateIdAsync(string candidateId);
    Task<DomainApplication> CreateAsync(DomainApplication application);
    Task<bool> UpdateAsync(string id, DomainApplication application);
    Task<bool> DeleteAsync(string id);
    Task<bool> HasAppliedAsync(string jobId, string candidateId);
}

