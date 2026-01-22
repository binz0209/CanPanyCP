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
    private readonly IJobService _jobService;
    private readonly IUserService _userService;
    private readonly IBackgroundEmailService _backgroundEmailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository repo,
        IJobService jobService,
        IUserService userService,
        IBackgroundEmailService backgroundEmailService,
        INotificationService notificationService,
        ILogger<ApplicationService> logger)
    {
        _repo = repo;
        _jobService = jobService;
        _userService = userService;
        _backgroundEmailService = backgroundEmailService;
        _notificationService = notificationService;
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

            // Fetch current application to check for status change
            var currentApp = await _repo.GetByIdAsync(id);
            if (currentApp == null)
                 throw new ArgumentException("Application not found", nameof(id));

            bool statusChanged = currentApp.Status != application.Status;
            
            // Id is already set, just update
            application.MarkAsUpdated();
            await _repo.UpdateAsync(application);

            if (statusChanged && (application.Status == "Accepted" || application.Status == "Rejected"))
            {
                try 
                {
                    // Fetch Job and Candidate info
                    var job = await _jobService.GetByIdAsync(application.JobId);
                    var candidate = await _userService.GetByIdAsync(application.CandidateId);

                    if (job != null && candidate != null)
                    {
                        // 1. Queue Email asynchronously
                        _backgroundEmailService.QueueApplicationStatusEmail(
                            candidate.Email, 
                            candidate.FullName, 
                            job.Title, 
                            application.Status);

                        // 2. Send In-App Notification
                        var notification = new Notification
                        {
                            UserId = application.CandidateId,
                            Type = "ApplicationUpdate",
                            Title = "Application Status Update",
                            Message = $"Your application for {job.Title} has been {application.Status}.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                        await _notificationService.CreateAsync(notification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send application status notification for AppId: {AppId}", id);
                    // Don't fail the update if notification fails
                }
            }

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

