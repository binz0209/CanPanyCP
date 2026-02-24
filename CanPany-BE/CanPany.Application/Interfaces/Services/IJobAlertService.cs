using CanPany.Application.DTOs.JobAlerts;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for managing job alerts
/// </summary>
public interface IJobAlertService
{
    // CRUD operations
    Task<JobAlertResponseDto> CreateAlertAsync(string userId, JobAlertCreateDto dto);
    Task<JobAlertResponseDto?> UpdateAlertAsync(string userId, string alertId, JobAlertUpdateDto dto);
    Task<bool> DeleteAlertAsync(string userId, string alertId);
    Task<bool> PauseAlertAsync(string userId, string alertId);
    Task<bool> ResumeAlertAsync(string userId, string alertId);
    Task<IEnumerable<JobAlertResponseDto>> GetUserAlertsAsync(string userId);
    Task<JobAlertResponseDto?> GetAlertByIdAsync(string userId, string alertId);
    
    // Matching logic
    Task<IEnumerable<Job>> FindMatchingJobsAsync(JobAlert alert, IEnumerable<Job> jobs);
    Task<int> GetMatchScoreAsync(JobAlert alert, Job job);
    Task<IEnumerable<JobAlert>> FindMatchingAlertsAsync(Job job);
    
    // Preview
    Task<IEnumerable<JobMatchInfo>> PreviewMatchesAsync(string userId, string alertId);
    
    // Stats
    Task<object> GetStatsAsync(string userId);
}

