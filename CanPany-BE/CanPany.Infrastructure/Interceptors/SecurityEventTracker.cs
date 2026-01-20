using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Security event tracker implementation with I18N support
/// </summary>
public class SecurityEventTracker : ISecurityEventTracker
{
    private readonly ILogger<SecurityEventTracker> _logger;
    private readonly IDataMasker _dataMasker;
    private readonly II18nService _i18nService;

    public SecurityEventTracker(
        ILogger<SecurityEventTracker> logger, 
        IDataMasker dataMasker,
        II18nService i18nService)
    {
        _logger = logger;
        _dataMasker = dataMasker;
        _i18nService = i18nService;
    }

    public Task TrackSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            // Mask sensitive data in metadata
            var maskedMetadata = securityEvent.Metadata != null
                ? _dataMasker.MaskSensitiveData(securityEvent.Metadata)
                : null;

            var logLevel = securityEvent.Severity switch
            {
                "CRITICAL" => LogLevel.Critical,
                "ERROR" => LogLevel.Error,
                "WARNING" => LogLevel.Warning,
                _ => LogLevel.Information
            };

            // Get i18n message based on event type
            var i18nKey = securityEvent.EventType switch
            {
                "AUTHENTICATION" => securityEvent.IsSuccess 
                    ? I18nKeys.Interceptor.Security.Format.AuthenticationSuccess
                    : I18nKeys.Interceptor.Security.Format.AuthenticationFailed,
                "AUTHORIZATION" => I18nKeys.Interceptor.Security.Format.AuthorizationDenied,
                "UNAUTHORIZED_ACCESS" => I18nKeys.Interceptor.Security.Format.UnauthorizedAccess,
                _ => I18nKeys.Interceptor.Security.Authentication
            };

            var logMessage = _i18nService.GetMessage(
                i18nKey,
                I18nContextType.Security,
                securityEvent.Action ?? "N/A",
                securityEvent.UserId ?? "Anonymous",
                securityEvent.ResourcePath ?? "N/A",
                securityEvent.IpAddress ?? "N/A",
                securityEvent.IsSuccess
            );

            _logger.Log(logLevel, logMessage);

            // Log metadata if present (masked)
            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[SECURITY_METADATA] {EventType} | {Metadata}",
                    securityEvent.EventType,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata)
                );
            }

            // Log failure reason if present
            if (!securityEvent.IsSuccess && !string.IsNullOrEmpty(securityEvent.FailureReason))
            {
                _logger.LogWarning(
                    "[SECURITY_FAILURE] {EventType} | {FailureReason}",
                    securityEvent.EventType,
                    securityEvent.FailureReason
                );
            }
        }
        catch (Exception ex)
        {
            // Don't let security tracking break the application
            _logger.LogError(ex, "Failed to track security event");
        }

        return Task.CompletedTask;
    }
}
