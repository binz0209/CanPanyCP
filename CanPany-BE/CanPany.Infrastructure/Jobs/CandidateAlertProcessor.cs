using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Jobs;

/// <summary>
/// Background job processor for candidate alerts (for companies)
/// </summary>
public class CandidateAlertProcessor
{
    private readonly ICandidateAlertRepository _alertRepo;
    private readonly IUserProfileRepository _profileRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly ICandidateMatchingService _matchingService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CandidateAlertProcessor> _logger;

    public CandidateAlertProcessor(
        ICandidateAlertRepository alertRepo,
        IUserProfileRepository profileRepo,
        IUserRepository userRepo,
        ICompanyRepository companyRepo,
        ICandidateMatchingService matchingService,
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<CandidateAlertProcessor> logger)
    {
        _alertRepo = alertRepo;
        _profileRepo = profileRepo;
        _userRepo = userRepo;
        _companyRepo = companyRepo;
        _matchingService = matchingService;
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Process daily candidate alerts - check for new profiles in last 24 hours
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessDailyCandidateAlertsAsync()
    {
        var runId = Guid.NewGuid().ToString().Substring(0, 8);
        try
        {
            _logger.LogInformation("[CandidateAlert-{RunId}] Starting daily candidate alert processing", runId);

            // Get all active alerts
            var activeAlerts = await _alertRepo.GetActiveAlertsAsync();
            var alertsList = activeAlerts.ToList();

            _logger.LogInformation("[CandidateAlert-{RunId}] Found {Count} active candidate alerts to process", runId, alertsList.Count);

            // Get new profiles from last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var newProfiles = await _profileRepo.GetProfilesCreatedAfterAsync(yesterday);
            var profilesList = newProfiles.ToList();

            _logger.LogInformation("[CandidateAlert-{RunId}] Found {Count} new profiles created since {Since}", runId, profilesList.Count, yesterday);

            if (!profilesList.Any())
            {
                _logger.LogInformation("[CandidateAlert-{RunId}] No new profiles to match. Skipping.", runId);
                return;
            }

            int totalMatches = 0;
            foreach (var alert in alertsList)
            {
                var matchesCount = await ProcessAlertAsync(alert, profilesList);
                totalMatches += matchesCount;
            }

            _logger.LogInformation("[CandidateAlert-{RunId}] Completed daily candidate alert processing. Total matches found: {TotalMatches}", runId, totalMatches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CandidateAlert-{RunId}] Error processing daily candidate alerts", runId);
            throw;
        }
    }

    private async Task<int> ProcessAlertAsync(CandidateAlert alert, List<UserProfile> profiles)
    {
        try
        {
            int matchesCount = 0;
            var company = await _companyRepo.GetByIdAsync(alert.CompanyId);
            if (company == null) return 0;

            foreach (var profile in profiles)
            {
                if (IsMatch(alert, profile))
                {
                    await SendMatchNotificationsAsync(alert, profile, company);
                    matchesCount++;
                }
            }

            if (matchesCount > 0)
            {
                _logger.LogInformation("Alert {AlertId} ('{Name}') matched {Count} candidates", alert.Id, alert.Name, matchesCount);
            }

            return matchesCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing candidate alert {AlertId}", alert.Id);
            return 0;
        }
    }

    private bool IsMatch(CandidateAlert alert, UserProfile profile)
    {
        // 1. Skill Match
        if (alert.SkillIds?.Any() == true)
        {
            if (profile.SkillIds == null || !profile.SkillIds.Any())
                return false;

            var hasMatchingSkill = alert.SkillIds.Any(id => profile.SkillIds.Contains(id));
            if (!hasMatchingSkill) return false;
        }

        // 2. Location Match
        if (!string.IsNullOrEmpty(alert.Location))
        {
            if (string.IsNullOrEmpty(profile.Location) || 
                !profile.Location.Contains(alert.Location, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // 3. Experience Match
        if (alert.MinExperience.HasValue || alert.MaxExperience.HasValue)
        {
            // Parse numeric experience if possible, otherwise skip or default
            // UserProfile.Experience is usually a string like "5 years"
            // For simplicity, we'll try a basic parse or skip if non-numeric
            var years = ParseExperienceYears(profile.Experience);
            
            if (alert.MinExperience.HasValue && years < alert.MinExperience.Value)
                return false;

            if (alert.MaxExperience.HasValue && years > alert.MaxExperience.Value)
                return false;
        }

        return true;
    }

    private int ParseExperienceYears(string? exp)
    {
        if (string.IsNullOrEmpty(exp)) return 0;
        
        // Basic extraction of first number
        var match = System.Text.RegularExpressions.Regex.Match(exp, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var result))
        {
            return result;
        }
        return 0;
    }

    private async Task SendMatchNotificationsAsync(CandidateAlert alert, UserProfile profile, Company company)
    {
        // 1. Fetch Candidate User to get FullName
        var candidateUser = await _userRepo.GetByIdAsync(profile.UserId);
        var candidateName = candidateUser?.FullName ?? "Candidate";

        // 2. In-App Notification to the company owner/recruiter
        // Assuming Company has a UserId for the account owner
        var companyUser = await _userRepo.GetByIdAsync(company.UserId);
        if (companyUser == null) return;

        var notification = new Notification
        {
            UserId = companyUser.Id,
            Type = "CandidateMatch",
            Title = "New Candidate Match!",
            Message = $"A new candidate matching your alert '{alert.Name}' has been found: {candidateName}",
            Payload = System.Text.Json.JsonSerializer.Serialize(new { profileId = profile.Id, alertId = alert.Id }),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationService.CreateAsync(notification);

        // 3. Email Notification if Company has an email (checking user email)
        if (!string.IsNullOrEmpty(companyUser.Email))
        {
            try
            {
                // Reusing a generic notification email template or adding a specific one
                await _emailService.SendNotificationEmailAsync(
                    companyUser.Email,
                    "New Candidate Match Found",
                    $"Hi {company.Name},\n\nWe found a new candidate matching your alert '{alert.Name}':\n\n- Name: {candidateName}\n- Title: {profile.Title}\n- Location: {profile.Location ?? "Remote"}\n- Experience: {profile.Experience}\n\nView profile: [Link to profile]");

                _logger.LogInformation("Sent candidate match notification to {Email}", companyUser.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send candidate match email to {Email}", companyUser.Email);
            }
        }
    }
}
