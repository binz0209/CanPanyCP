using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Contract management service — creates contracts from accepted applications,
/// manages lifecycle transitions (Active → InProgress → Completed/Cancelled).
/// </summary>
public class ContractService : IContractService
{
    private readonly IContractRepository _contractRepo;
    private readonly IApplicationRepository _applicationRepo;
    private readonly ILogger<ContractService> _logger;

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active", "InProgress", "Completed", "Cancelled", "Disputed", "Resolved"
    };

    public ContractService(
        IContractRepository contractRepo,
        IApplicationRepository applicationRepo,
        ILogger<ContractService> logger)
    {
        _contractRepo = contractRepo;
        _applicationRepo = applicationRepo;
        _logger = logger;
    }

    public async Task<Contract?> GetByIdAsync(string id)
    {
        return await _contractRepo.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Contract>> GetByCompanyIdAsync(string companyId)
    {
        return await _contractRepo.GetByCompanyIdAsync(companyId);
    }

    public async Task<IEnumerable<Contract>> GetByCandidateIdAsync(string candidateId)
    {
        return await _contractRepo.GetByCandidateIdAsync(candidateId);
    }

    public async Task<Contract> CreateFromApplicationAsync(
        string applicationId, string companyId, decimal agreedAmount,
        DateTime? startDate, DateTime? endDate)
    {
        // Verify the application exists
        var application = await _applicationRepo.GetByIdAsync(applicationId)
            ?? throw new InvalidOperationException($"Application {applicationId} not found");

        // Ensure application is accepted before creating contract
        if (application.Status != "Accepted" && application.Status != "Interviewed")
        {
            throw new InvalidOperationException($"Application must be Accepted or Interviewed to create a contract. Current status: {application.Status}");
        }

        // Check for existing contract
        var existing = await _contractRepo.GetByApplicationIdAsync(applicationId);
        if (existing != null)
        {
            throw new InvalidOperationException($"Contract already exists for application {applicationId}");
        }

        var contract = new Contract
        {
            JobId = application.JobId,
            ApplicationId = applicationId,
            CompanyId = companyId,
            CandidateId = application.CandidateId,
            AgreedAmount = agreedAmount,
            Status = "Active",
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _contractRepo.AddAsync(contract);
        _logger.LogInformation("Contract created: {ContractId} for application {ApplicationId}",
            created.Id, applicationId);

        return created;
    }

    public async Task UpdateStatusAsync(string id, string status, string? cancellationReason = null)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new ArgumentException($"Invalid contract status: {status}. Valid statuses: {string.Join(", ", ValidStatuses)}");
        }

        var contract = await _contractRepo.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Contract {id} not found");

        contract.Status = status;

        if (status == "Completed")
            contract.CompletedAt = DateTime.UtcNow;

        if (status == "Cancelled")
        {
            contract.CancelledAt = DateTime.UtcNow;
            contract.CancellationReason = cancellationReason;
        }

        await _contractRepo.UpdateAsync(contract);
        _logger.LogInformation("Contract {ContractId} status updated to {Status}", id, status);
    }
}
