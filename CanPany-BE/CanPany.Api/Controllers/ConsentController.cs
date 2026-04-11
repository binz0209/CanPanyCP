using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Consent controller — Privacy/data consent management (Nghị định 13/2023/NĐ-CP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsentController : ControllerBase
{
    private readonly IConsentService _consentService;
    private readonly ILogger<ConsentController> _logger;

    public ConsentController(
        IConsentService consentService,
        ILogger<ConsentController> logger)
    {
        _consentService = consentService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/consent — Get all consent records for the authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyConsents()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var consents = await _consentService.GetUserConsentsAsync(userId);
            return Ok(ApiResponse<IEnumerable<UserConsent>>.CreateSuccess(consents));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consents");
            return StatusCode(500, ApiResponse.CreateError("Failed to get consents", "GetConsentsFailed"));
        }
    }

    /// <summary>
    /// POST /api/consent — Grant a consent.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GrantConsent([FromBody] GrantConsentRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var consent = await _consentService.GrantConsentAsync(
                userId,
                request.ConsentType,
                request.PolicyVersion,
                ipAddress);

            return Ok(ApiResponse<UserConsent>.CreateSuccess(consent, "Consent granted"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.CreateError(ex.Message, "InvalidConsentType"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error granting consent");
            return StatusCode(500, ApiResponse.CreateError("Failed to grant consent", "GrantConsentFailed"));
        }
    }

    /// <summary>
    /// DELETE /api/consent/{consentType} — Revoke a consent.
    /// </summary>
    [HttpDelete("{consentType}")]
    public async Task<IActionResult> RevokeConsent(string consentType)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _consentService.RevokeConsentAsync(userId, consentType);
            return Ok(ApiResponse.CreateSuccess("Consent revoked"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent: {ConsentType}", consentType);
            return StatusCode(500, ApiResponse.CreateError("Failed to revoke consent", "RevokeConsentFailed"));
        }
    }

    /// <summary>
    /// GET /api/consent/check/{consentType} — Check if a specific consent is currently granted.
    /// </summary>
    [HttpGet("check/{consentType}")]
    public async Task<IActionResult> CheckConsent(string consentType)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var hasConsent = await _consentService.HasConsentAsync(userId, consentType);
            return Ok(ApiResponse<object>.CreateSuccess(new { consentType, isGranted = hasConsent }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking consent: {ConsentType}", consentType);
            return StatusCode(500, ApiResponse.CreateError("Failed to check consent", "CheckConsentFailed"));
        }
    }
}

public record GrantConsentRequest(string ConsentType, string? PolicyVersion);
