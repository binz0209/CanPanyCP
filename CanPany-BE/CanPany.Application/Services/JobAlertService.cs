using CanPany.Application.DTOs.JobAlerts;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Job Alert service implementation
/// </summary>
public class JobAlertService : IJobAlertService
{
    private readonly IJobAlertRepository _alertRepo;
    private readonly IJobAlertMatchRepository _matchRepo;
    private readonly IJobRepository _jobRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly ISkillRepository _skillRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ILogger<JobAlertService> _logger;

    public JobAlertService(
        IJobAlertRepository alertRepo,
        IJobAlertMatchRepository matchRepo,
        IJobRepository jobRepo,
        ICompanyRepository companyRepo,
        ISkillRepository skillRepo,
        ICategoryRepository categoryRepo,
        ILogger<JobAlertService> logger)
    {
        _alertRepo = alertRepo;
        _matchRepo = matchRepo;
        _jobRepo = jobRepo;
        _companyRepo = companyRepo;
        _skillRepo = skillRepo;
        _categoryRepo = categoryRepo;
        _logger = logger;
    }

    public async Task<JobAlertResponseDto> CreateAlertAsync(string userId, JobAlertCreateDto dto)
    {
        try
        {
            var alert = new JobAlert
            {
                UserId = userId,
                Title = dto.Title?.Trim(),
                SkillIds = dto.SkillIds,
                CategoryIds = dto.CategoryIds,
                Location = dto.Location?.Trim(),
                JobType = dto.JobType?.Trim(),
                MinBudget = dto.MinBudget,
                MaxBudget = dto.MaxBudget,
                ExperienceLevel = dto.ExperienceLevel?.Trim(),
                Frequency = dto.Frequency,
                EmailEnabled = dto.EmailEnabled,
                InAppEnabled = dto.InAppEnabled,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _alertRepo.AddAsync(alert);
            _logger.LogInformation("Job alert created for user {UserId}: {AlertId}", userId, created.Id);

            return MapToResponseDto(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job alert for user {UserId}", userId);
            throw;
        }
    }

    public async Task<JobAlertResponseDto?> UpdateAlertAsync(string userId, string alertId, JobAlertUpdateDto dto)
    {
        try
        {
            var alert = await _alertRepo.GetByIdAsync(alertId);
            if (alert == null || alert.UserId != userId)
            {
                _logger.LogWarning("Job alert {AlertId} not found for user {UserId}", alertId, userId);
                return null;
            }

            // Update fields if provided
            if (dto.Title != null) alert.Title = dto.Title.Trim();
            if (dto.SkillIds != null) alert.SkillIds = dto.SkillIds;
            if (dto.CategoryIds != null) alert.CategoryIds = dto.CategoryIds;
            if (dto.Location != null) alert.Location = dto.Location.Trim();
            if (dto.JobType != null) alert.JobType = dto.JobType.Trim();
            if (dto.MinBudget.HasValue) alert.MinBudget = dto.MinBudget;
            if (dto.MaxBudget.HasValue) alert.MaxBudget = dto.MaxBudget;
            if (dto.ExperienceLevel != null) alert.ExperienceLevel = dto.ExperienceLevel.Trim();
            if (dto.Frequency != null) alert.Frequency = dto.Frequency;
            if (dto.EmailEnabled.HasValue) alert.EmailEnabled = dto.EmailEnabled.Value;
            if (dto.InAppEnabled.HasValue) alert.InAppEnabled = dto.InAppEnabled.Value;

            alert.UpdatedAt = DateTime.UtcNow;
            await _alertRepo.UpdateAsync(alert);

            _logger.LogInformation("Job alert {AlertId} updated for user {UserId}", alertId, userId);
            return MapToResponseDto(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job alert {AlertId} for user {UserId}", alertId, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAlertAsync(string userId, string alertId)
    {
        try
        {
            var alert = await _alertRepo.GetByIdAsync(alertId);
            if (alert == null || alert.UserId != userId)
            {
                _logger.LogWarning("Job alert {AlertId} not found for user {UserId}", alertId, userId);
                return false;
            }

            await _alertRepo.DeleteAsync(alertId);
            _logger.LogInformation("Job alert {AlertId} deleted for user {UserId}", alertId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job alert {AlertId} for user {UserId}", alertId, userId);
            throw;
        }
    }

    public async Task<bool> PauseAlertAsync(string userId, string alertId)
    {
        return await ToggleAlertStatusAsync(userId, alertId, false);
    }

    public async Task<bool> ResumeAlertAsync(string userId, string alertId)
    {
        return await ToggleAlertStatusAsync(userId, alertId, true);
    }

    private async Task<bool> ToggleAlertStatusAsync(string userId, string alertId, bool isActive)
    {
        try
        {
            var alert = await _alertRepo.GetByIdAsync(alertId);
            if (alert == null || alert.UserId != userId)
            {
                _logger.LogWarning("Job alert {AlertId} not found for user {UserId}", alertId, userId);
                return false;
            }

            alert.IsActive = isActive;
            alert.UpdatedAt = DateTime.UtcNow;
            await _alertRepo.UpdateAsync(alert);

            var action = isActive ? "resumed" : "paused";
            _logger.LogInformation("Job alert {AlertId} {Action} for user {UserId}", alertId, action, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling job alert {AlertId} for user {UserId}", alertId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<JobAlertResponseDto>> GetUserAlertsAsync(string userId)
    {
        try
        {
            var alerts = await _alertRepo.GetByUserIdAsync(userId);
            return alerts.Select(MapToResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job alerts for user {UserId}", userId);
            throw;
        }
    }

    public async Task<JobAlertResponseDto?> GetAlertByIdAsync(string userId, string alertId)
    {
        try
        {
            var alert = await _alertRepo.GetByIdAsync(alertId);
            if (alert == null || alert.UserId != userId)
            {
                return null;
            }

            return MapToResponseDto(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job alert {AlertId} for user {UserId}", alertId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<Job>> FindMatchingJobsAsync(JobAlert alert, IEnumerable<Job> jobs)
    {
        var matchedJobs = new List<Job>();

        foreach (var job in jobs)
        {
            if (await IsJobMatchingAlertAsync(alert, job))
            {
                matchedJobs.Add(job);
            }
        }

        return matchedJobs;
    }

    private async Task<bool> IsJobMatchingAlertAsync(JobAlert alert, Job job)
    {
        // Check if already matched
        if (await _matchRepo.MatchExistsAsync(alert.Id, job.Id))
        {
            _logger.LogInformation("ℹ Match already exists for Alert {AlertId} and Job {JobId}. Skipping.", alert.Id, job.Id);
            return false;
        }

        // Filter by skills
        if (alert.SkillIds?.Any() == true)
        {
            if (job.SkillIds == null || !job.SkillIds.Any()) 
            {
                _logger.LogInformation("∅ Skill mismatch: Job {JobId} has no skills", job.Id);
                return false;
            }

            _logger.LogInformation("🔍 Comparing Alert Skill IDs [{AlertSkills}] with Job Skill Strings [{JobSkills}]", 
                string.Join(", ", alert.SkillIds), string.Join(", ", job.SkillIds));

            // Smart matching: Try ID match first, then Name match
            bool hasMatchingSkill = false;
            foreach (var skillId in alert.SkillIds)
            {
                // Direct ID match
                if (job.SkillIds.Contains(skillId))
                {
                    hasMatchingSkill = true;
                    break;
                }

                // Name-based match (Resolve ID to Name)
                var skill = await _skillRepo.GetByIdAsync(skillId);
                if (skill != null && job.SkillIds.Any(js => js.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogInformation("🎯 Smart Match: Job skill '{JobSkill}' matches Alert skill '{SkillName}' (Resolved from ID {SkillId})", 
                        skill.Name, skill.Name, skillId);
                    hasMatchingSkill = true;
                    break;
                }
            }
            
            if (!hasMatchingSkill)
            {
                _logger.LogInformation("∅ Skill mismatch: Job {JobId} skills do not match alert {AlertId}", job.Id, alert.Id);
                return false;
            }
        }

        // Filter by category
        if (alert.CategoryIds?.Any() == true)
        {
            _logger.LogInformation("🔍 Checking Categories for Job {JobId} (Current: '{JobCat}')", job.Id, job.CategoryId ?? "null");

            bool hasMatchingCategory = false;
            foreach (var categoryId in alert.CategoryIds)
            {
                // Direct ID match
                if (job.CategoryId == categoryId)
                {
                    hasMatchingCategory = true;
                    break;
                }

                // Name-based match (Resolve ID to Name)
                var category = await _categoryRepo.GetByIdAsync(categoryId);
                if (category != null)
                {
                    _logger.LogInformation("🔍 Checking if Job Category '{JobCat}' matches Alert Category Name '{CatName}'", 
                        job.CategoryId ?? "null", category.Name);
                        
                    if (job.CategoryId?.Equals(category.Name, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        _logger.LogInformation("🎯 Smart Match: Job category '{JobCat}' matches Alert category '{CatName}'", 
                            job.CategoryId, category.Name);
                        hasMatchingCategory = true;
                        break;
                    }
                }
            }

            if (!hasMatchingCategory)
            {
                _logger.LogInformation("∅ Category mismatch: Job {JobId} category '{Cat}' not in alert categories", job.Id, job.CategoryId ?? "null");
                return false;
            }
        }

        // Filter by location
        if (!string.IsNullOrEmpty(alert.Location))
        {
            if (string.IsNullOrEmpty(job.Location) || 
                !job.Location.Contains(alert.Location, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("∅ Location mismatch: Job {JobId} location '{Loc}' vs alert '{AlertLoc}'", job.Id, job.Location, alert.Location);
                return false;
            }
        }

        // Filter by job type (using EngagementType from Job entity)
        if (!string.IsNullOrEmpty(alert.JobType))
        {
            bool isTypeMatch = job.EngagementType == alert.JobType;
            
            // Lenient mapping: FullTime == DirectHire, Freelance == FreelanceProject
            if (!isTypeMatch)
            {
                if (alert.JobType == "FullTime" && job.EngagementType == "DirectHire") isTypeMatch = true;
                if (alert.JobType == "Freelance" && job.EngagementType == "FreelanceProject") isTypeMatch = true;
            }

            if (!isTypeMatch)
            {
                _logger.LogInformation("∅ JobType mismatch: Job {JobId} type '{Type}' vs alert '{AlertType}'", job.Id, job.EngagementType, alert.JobType);
                return false;
            }
        }

        // Filter by budget
        if (alert.MinBudget.HasValue && (job.BudgetAmount ?? 0) < alert.MinBudget.Value)
        {
            _logger.LogInformation("∅ Budget mismatch: Job {JobId} amount {Amt} < min {Min}", job.Id, job.BudgetAmount, alert.MinBudget);
            return false;
        }

        if (alert.MaxBudget.HasValue && (job.BudgetAmount ?? 0) > alert.MaxBudget.Value)
        {
            _logger.LogInformation("∅ Budget mismatch: Job {JobId} amount {Amt} > max {Max}", job.Id, job.BudgetAmount, alert.MaxBudget);
            return false;
        }

        // Filter by experience level (using Level from Job entity)
        if (!string.IsNullOrEmpty(alert.ExperienceLevel))
        {
            if (job.Level != alert.ExperienceLevel)
            {
                _logger.LogInformation("∅ Level mismatch: Job {JobId} level '{Level}' vs alert '{AlertLevel}'", job.Id, job.Level, alert.ExperienceLevel);
                return false;
            }
        }

        return true;
    }

    public async Task<int> GetMatchScoreAsync(JobAlert alert, Job job)
    {
        int score = 0;

        // Skills match (40 points)
        if (alert.SkillIds?.Any() == true && job.SkillIds?.Any() == true) // ? Changed
        {
            var matchCount = alert.SkillIds.Intersect(job.SkillIds).Count(); // ? Changed
            var skillScore = (int)(40 * (matchCount / (double)alert.SkillIds.Count));
            score += skillScore;
        }

        // Location match (20 points)
        if (!string.IsNullOrEmpty(alert.Location) && 
            job.Location?.Contains(alert.Location, StringComparison.OrdinalIgnoreCase) == true)
        {
            score += 20;
        }

        // Budget match (20 points)
        if (alert.MinBudget.HasValue || alert.MaxBudget.HasValue)
        {
            var jobBudget = job.BudgetAmount ?? 0; // ? Changed from Budget
            if (jobBudget >= (alert.MinBudget ?? 0) && 
                jobBudget <= (alert.MaxBudget ?? decimal.MaxValue))
            {
                score += 20;
            }
        }

        // Job type match (10 points) - using EngagementType
        if (!string.IsNullOrEmpty(alert.JobType) && alert.JobType == job.EngagementType) // ? Changed
        {
            score += 10;
        }

        // Experience level match (10 points) - using Level
        if (!string.IsNullOrEmpty(alert.ExperienceLevel) && 
            alert.ExperienceLevel == job.Level) // ? Changed
        {
            score += 10;
        }

        return score;
    }

    public async Task<IEnumerable<JobMatchInfo>> PreviewMatchesAsync(string userId, string alertId)
    {
        try
        {
            var alert = await _alertRepo.GetByIdAsync(alertId);
            if (alert == null || alert.UserId != userId)
            {
                return Enumerable.Empty<JobMatchInfo>();
            }

            // Get active jobs from last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentJobs = await _jobRepo.GetJobsCreatedAfterAsync(thirtyDaysAgo);
            var activeJobs = recentJobs.Where(j => j.Status == "Open").ToList(); // ? Changed "Active" to "Open"

            var matches = new List<JobMatchInfo>();

            foreach (var job in activeJobs)
            {
                if (await IsJobMatchingAlertAsync(alert, job))
                {
                    var company = await _companyRepo.GetByIdAsync(job.CompanyId);
                    var matchScore = await GetMatchScoreAsync(alert, job);

                    matches.Add(new JobMatchInfo(
                        job.Id,
                        job.Title,
                        company?.Name ?? "Unknown", // ? CHANGED from CompanyName
                        job.Location ?? "Remote",
                        job.BudgetAmount.HasValue ? $"{job.BudgetAmount.Value:N0} VND" : "Negotiable",
                        matchScore
                    ));
                }
            }

            return matches.OrderByDescending(m => m.MatchScore).Take(10);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing matches for alert {AlertId}", alertId);
            throw;
        }
    }

    public async Task<object> GetStatsAsync(string userId)
    {
        try
        {
            var alerts = await _alertRepo.GetByUserIdAsync(userId);
            var alertsList = alerts.ToList();

            var totalAlerts = alertsList.Count;
            var activeAlerts = alertsList.Count(a => a.IsActive);
            var totalMatches = alertsList.Sum(a => a.MatchCount);

            // Get recent matches (last 7 days)
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var recentMatches = await _matchRepo.GetByUserIdAsync(userId, sevenDaysAgo);

            return new
            {
                totalAlerts,
                activeAlerts,
                pausedAlerts = totalAlerts - activeAlerts,
                totalMatches,
                recentMatches = recentMatches.Count()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stats for user {UserId}", userId);
            throw;
        }
    }

    private JobAlertResponseDto MapToResponseDto(JobAlert alert)
    {
        return new JobAlertResponseDto(
            alert.Id,
            alert.UserId,
            alert.Title,
            alert.SkillIds,
            alert.CategoryIds,
            alert.Location,
            alert.JobType,
            alert.MinBudget,
            alert.MaxBudget,
            alert.ExperienceLevel,
            alert.IsActive,
            alert.Frequency,
            alert.EmailEnabled,
            alert.InAppEnabled,
            alert.LastTriggeredAt,
            alert.MatchCount,
            alert.CreatedAt
        );
    }

    public async Task<IEnumerable<JobAlert>> FindMatchingAlertsAsync(Job job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        try
        {
            // Get all active alerts
            var allAlerts = await _alertRepo.GetActiveAlertsAsync();
            var matchingAlerts = new List<JobAlert>();

            foreach (var alert in allAlerts)
            {
                if (await IsJobMatchingAlertAsync(alert, job))
                {
                    matchingAlerts.Add(alert);
                }
            }

            return matchingAlerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching alerts for job {JobId}", job.Id);
            throw;
        }
    }
}

