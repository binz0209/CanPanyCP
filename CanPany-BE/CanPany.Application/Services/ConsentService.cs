using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Consent management service for Vietnamese data protection law compliance (Nghị định 13/2023/NĐ-CP).
/// Tracks user consent for data processing, cross-border transfers, and AI analysis.
/// </summary>
public class ConsentService : IConsentService
{
    private readonly IUserConsentRepository _consentRepo;
    private readonly ILogger<ConsentService> _logger;

    /// <summary>All valid consent types supported by the system.</summary>
    public static readonly HashSet<string> ValidConsentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DataProcessing",
        "CrossBorderTransfer",
        "AIAnalysis",
        "ExternalSync_GitHub",
        "ExternalSync_LinkedIn",
        "Marketing"
    };

    public ConsentService(
        IUserConsentRepository consentRepo,
        ILogger<ConsentService> logger)
    {
        _consentRepo = consentRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<UserConsent>> GetUserConsentsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        return await _consentRepo.GetByUserIdAsync(userId);
    }

    public async Task<UserConsent> GrantConsentAsync(string userId, string consentType, string? policyVersion, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        if (!ValidConsentTypes.Contains(consentType))
            throw new ArgumentException($"Invalid consent type: {consentType}. Valid types: {string.Join(", ", ValidConsentTypes)}");

        // Check for existing consent of the same type
        var existing = await _consentRepo.GetByUserAndTypeAsync(userId, consentType);

        if (existing != null)
        {
            // Re-grant if previously revoked
            existing.IsGranted = true;
            existing.GrantedAt = DateTime.UtcNow;
            existing.RevokedAt = null;
            existing.PolicyVersion = policyVersion;
            existing.IpAddress = ipAddress;
            existing.UpdatedAt = DateTime.UtcNow;
            await _consentRepo.UpdateAsync(existing);

            _logger.LogInformation("Consent re-granted: {UserId} - {ConsentType}", userId, consentType);
            return existing;
        }

        var consent = new UserConsent
        {
            UserId = userId,
            ConsentType = consentType,
            IsGranted = true,
            GrantedAt = DateTime.UtcNow,
            PolicyVersion = policyVersion,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _consentRepo.AddAsync(consent);
        _logger.LogInformation("Consent granted: {UserId} - {ConsentType}", userId, consentType);

        return created;
    }

    public async Task RevokeConsentAsync(string userId, string consentType)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        var consent = await _consentRepo.GetByUserAndTypeAsync(userId, consentType);
        if (consent == null)
        {
            _logger.LogWarning("Consent not found for revocation: {UserId} - {ConsentType}", userId, consentType);
            return;
        }

        consent.IsGranted = false;
        consent.RevokedAt = DateTime.UtcNow;
        consent.UpdatedAt = DateTime.UtcNow;
        await _consentRepo.UpdateAsync(consent);

        _logger.LogInformation("Consent revoked: {UserId} - {ConsentType}", userId, consentType);
    }

    public async Task<bool> HasConsentAsync(string userId, string consentType)
    {
        return await _consentRepo.HasConsentAsync(userId, consentType);
    }
}
