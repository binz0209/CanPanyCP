using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Job service implementation
/// </summary>
public class JobService : IJobService
{
    private readonly IJobRepository _repo;
    private readonly IJobMatchingService _jobMatchingService;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository repo,
        IJobMatchingService jobMatchingService,
        ILogger<JobService> logger)
    {
        _repo = repo;
        _jobMatchingService = jobMatchingService;
        _logger = logger;
    }

    public async Task<Job?> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(id));

            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job by ID: {JobId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        try
        {
            return await _repo.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all jobs");
            throw;
        }
    }

    public async Task<IEnumerable<Job>> GetByCompanyIdAsync(string companyId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Company ID cannot be null or empty", nameof(companyId));

            return await _repo.GetByCompanyIdAsync(companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs by company ID: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<IEnumerable<Job>> GetByStatusAsync(string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            return await _repo.GetByStatusAsync(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs by status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Job>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget)
    {
        try
        {
            return await _repo.SearchAsync(keyword, categoryId, skillIds, minBudget, maxBudget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching jobs");
            throw;
        }
    }

    public async Task<Job> CreateAsync(Job job)
    {
        try
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            job.CreatedAt = DateTime.UtcNow;
            var createdJob = await _repo.AddAsync(job);

            // Trigger job alert matching in background
            _jobMatchingService.TriggerJobAlertMatching(createdJob.Id);

            return createdJob;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, Job job)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(id));
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            // Id is already set, just update
            job.MarkAsUpdated();
            await _repo.UpdateAsync(job);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job: {JobId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(id));

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job: {JobId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(id));

            return await _repo.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking job existence: {JobId}", id);
            throw;
        }
    }
}


