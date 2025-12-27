using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Application.Services;

/// <summary>
/// Application service implementation
/// </summary>
public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repo;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository repo,
        ILogger<ApplicationService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<DomainApplication?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Application ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application by ID: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DomainApplication>> GetByJobIdAsync(string jobId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            return await _repo.GetByJobIdAsync(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applications by job ID: {JobId}", jobId);
            throw;
        }
    }

    public async Task<IEnumerable<DomainApplication>> GetByCandidateIdAsync(string candidateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(candidateId))
                throw new ArgumentException("Candidate ID cannot be null or empty", nameof(candidateId));

            return await _repo.GetByCandidateIdAsync(candidateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting applications by candidate ID: {CandidateId}", candidateId);
            throw;
        }
    }

    public async Task<DomainApplication> CreateAsync(DomainApplication application)
    {
        try
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.CreatedAt = DateTime.UtcNow;
            return await _repo.AddAsync(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, DomainApplication application)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Application ID cannot be null or empty", nameof(id));
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            // Id is already set, just update
            application.MarkAsUpdated();
            await _repo.UpdateAsync(application);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Application ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<bool> HasAppliedAsync(string jobId, string candidateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));
            if (string.IsNullOrWhiteSpace(candidateId))
                throw new ArgumentException("Candidate ID cannot be null or empty", nameof(candidateId));

            return await _repo.HasAppliedAsync(jobId, candidateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if candidate has applied: {JobId}, {CandidateId}", jobId, candidateId);
            throw;
        }
    }
}

