using CanPany.Application.DTOs;
using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service for calculating match scores between candidates and jobs
/// </summary>
public interface ICandidateMatchingService
{
    /// <summary>
    /// Calculates the compatibility score between a job and a candidate profile
    /// </summary>
    /// <param name="job">The job requirements</param>
    /// <param name="profile">The candidate profile</param>
    /// <returns>Matching result with score breakdown</returns>
    MatchingResultDto CalculateMatchScore(Job job, UserProfile profile);
}
