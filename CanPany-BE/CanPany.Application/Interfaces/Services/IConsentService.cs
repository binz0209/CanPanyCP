using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for Privacy/Consent management (Nghị định 13/2023)
/// </summary>
public interface IConsentService
{
    Task<IEnumerable<UserConsent>> GetUserConsentsAsync(string userId);
    Task<UserConsent> GrantConsentAsync(string userId, string consentType, string? policyVersion, string? ipAddress);
    Task RevokeConsentAsync(string userId, string consentType);
    Task<bool> HasConsentAsync(string userId, string consentType);
}
