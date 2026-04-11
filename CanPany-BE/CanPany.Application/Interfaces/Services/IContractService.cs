using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for Contract management
/// </summary>
public interface IContractService
{
    Task<Contract?> GetByIdAsync(string id);
    Task<IEnumerable<Contract>> GetByCompanyIdAsync(string companyId);
    Task<IEnumerable<Contract>> GetByCandidateIdAsync(string candidateId);
    Task<Contract> CreateFromApplicationAsync(string applicationId, string companyId, decimal agreedAmount, DateTime? startDate, DateTime? endDate);
    Task UpdateStatusAsync(string id, string status, string? cancellationReason = null);
}
