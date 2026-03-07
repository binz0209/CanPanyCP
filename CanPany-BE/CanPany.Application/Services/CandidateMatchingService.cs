using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Service for calculating match scores between candidates and jobs
/// </summary>
public class CandidateMatchingService : ICandidateMatchingService
{
    private readonly ILogger<CandidateMatchingService> _logger;

    public CandidateMatchingService(ILogger<CandidateMatchingService> logger)
    {
        _logger = logger;
    }

    public MatchingResultDto CalculateMatchScore(Job job, UserProfile profile)
    {
        var result = new MatchingResultDto();

        try
        {
            // 1. Skill Match (50%)
            result.Breakdown.SkillMatch = CalculateSkillScore(job, profile);

            // 2. Experience Match (30%)
            result.Breakdown.ExperienceMatch = CalculateExperienceScore(job, profile);

            // 3. Location Match (20%)
            result.Breakdown.LocationMatch = CalculateLocationScore(job, profile);

            // Total Score calculation
            result.TotalMatchScore = Math.Round(
                (result.Breakdown.SkillMatch * 0.5) +
                (result.Breakdown.ExperienceMatch * 0.3) +
                (result.Breakdown.LocationMatch * 0.2), 
                2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating match score for Job {JobId} and Profile {ProfileId}", job.Id, profile.Id);
            // Fallback to 0 but don't throw
        }

        return result;
    }

    private double CalculateSkillScore(Job job, UserProfile profile)
    {
        if (job.SkillIds == null || !job.SkillIds.Any())
        {
            return 100.0; // No requirements = 100% match
        }

        if (profile.SkillIds == null || !profile.SkillIds.Any())
        {
            return 0.0;
        }

        var matchedSkills = job.SkillIds.Intersect(profile.SkillIds).Count();
        return (double)matchedSkills / job.SkillIds.Count * 100.0;
    }

    private double CalculateExperienceScore(Job job, UserProfile profile)
    {
        if (string.IsNullOrEmpty(job.Level))
        {
            return 100.0; // No level requirement = 100% match
        }

        var jobLevel = ParseJobLevel(job.Level);
        var profileLevel = InferProfileLevel(profile);

        if (profileLevel >= jobLevel)
        {
            return 100.0;
        }

        var diff = (int)jobLevel - (int)profileLevel;
        return diff switch
        {
            1 => 70.0,
            _ => 40.0
        };
    }

    private double CalculateLocationScore(Job job, UserProfile profile)
    {
        // Remote Job Case
        if (job.IsRemote)
        {
            // If profile has "remote" in location or bio, or if we assume all candidates are remote-friendly
            // by default in this platform. For now, high score if job is remote.
            return 100.0; 
        }

        // Onsite Job Case
        if (string.IsNullOrEmpty(job.Location) || string.IsNullOrEmpty(profile.Location))
        {
            return 50.0; // Partial match if info is missing
        }

        if (job.Location.Equals(profile.Location, StringComparison.OrdinalIgnoreCase))
        {
            return 100.0;
        }

        // Check if same country (simplistic check for now, assuming "City, Country" format or common names)
        if (IsSameCountry(job.Location, profile.Location))
        {
            return 70.0;
        }

        return 30.0;
    }

    private JobLevel ParseJobLevel(string level)
    {
        if (Enum.TryParse<JobLevel>(level, true, out var result))
        {
            return result;
        }
        return JobLevel.Junior; // Default to lowest if unparseable
    }

    private JobLevel InferProfileLevel(UserProfile profile)
    {
        // Combine Title and Experience string to infer level
        var textToSearch = $"{profile.Title} {profile.Experience}".ToLower();

        if (textToSearch.Contains("expert") || textToSearch.Contains("lead") || textToSearch.Contains("principal") || textToSearch.Contains("architect"))
            return JobLevel.Expert;
        
        if (textToSearch.Contains("senior") || textToSearch.Contains("sr"))
            return JobLevel.Senior;

        if (textToSearch.Contains("middle") || textToSearch.Contains("mid"))
            return JobLevel.Mid;

        return JobLevel.Junior; // Default
    }

    private bool IsSameCountry(string loc1, string loc2)
    {
        // Very basic heuristic for this implementation
        var parts1 = loc1.Split(',', StringSplitOptions.TrimEntries);
        var parts2 = loc2.Split(',', StringSplitOptions.TrimEntries);

        if (parts1.Length > 1 && parts2.Length > 1)
        {
            return parts1.Last().Equals(parts2.Last(), StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
