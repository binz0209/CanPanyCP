using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Job alert service implementation
/// </summary>
public class JobAlertService : IJobAlertService
{
    private readonly IJobAlertRepository _repo;
    private readonly ILogger<JobAlertService> _logger;

    public JobAlertService(
        IJobAlertRepository repo,
        ILogger<JobAlertService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<JobAlert>> GetActiveAlertsAsync()
    {
        try
        {
            var allAlerts = await _repo.GetAllAsync();
            return allAlerts.Where(a => a.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active job alerts");
            throw;
        }
    }

    public async Task<IEnumerable<JobAlert>> GetActiveAlertsByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _repo.GetActiveAlertsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active job alerts for user: {UserId}", userId);
            throw;
        }
    }

    public bool CheckJobMatchesAlert(Job job, JobAlert alert)
    {
        try
        {
            // Check category match
            if (alert.CategoryId != null && job.CategoryId != alert.CategoryId)
            {
                return false;
            }

            // Check location match (case-insensitive partial match)
            if (!string.IsNullOrWhiteSpace(alert.Location) && 
                !string.IsNullOrWhiteSpace(job.Location) &&
                !job.Location.Contains(alert.Location, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check remote preference
            if (alert.IsRemote.HasValue && job.IsRemote != alert.IsRemote.Value)
            {
                return false;
            }

            // Check budget range
            if (job.BudgetAmount.HasValue)
            {
                if (alert.MinBudget.HasValue && job.BudgetAmount < alert.MinBudget)
                {
                    return false;
                }

                if (alert.MaxBudget.HasValue && job.BudgetAmount > alert.MaxBudget)
                {
                    return false;
                }
            }

            // Check skills match (at least one skill must match)
            if (alert.SkillIds != null && alert.SkillIds.Any())
            {
                if (job.SkillIds == null || !job.SkillIds.Any())
                {
                    return false;
                }

                var hasMatchingSkill = alert.SkillIds.Any(alertSkill => 
                    job.SkillIds.Contains(alertSkill));

                if (!hasMatchingSkill)
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking job match for JobId: {JobId}, AlertId: {AlertId}", 
                job.Id, alert.Id);
            return false;
        }
    }

    public async Task<IEnumerable<JobAlert>> FindMatchingAlertsAsync(Job job)
    {
        try
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            var activeAlerts = await GetActiveAlertsAsync();
            var matchingAlerts = activeAlerts.Where(alert => CheckJobMatchesAlert(job, alert)).ToList();

            _logger.LogInformation(
                "Found {Count} matching alerts for job {JobId}", 
                matchingAlerts.Count, 
                job.Id);

            return matchingAlerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching alerts for job: {JobId}", job.Id);
            throw;
        }
    }
}

