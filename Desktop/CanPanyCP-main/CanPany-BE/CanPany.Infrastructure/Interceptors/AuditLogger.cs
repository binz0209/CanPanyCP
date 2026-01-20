using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Audit logger implementation with I18N support
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IDataMasker _dataMasker;
    private readonly II18nService _i18nService;

    public AuditLogger(
        ILogger<AuditLogger> logger, 
        IDataMasker dataMasker,
        II18nService i18nService)
    {
        _logger = logger;
        _dataMasker = dataMasker;
        _i18nService = i18nService;
    }

    public Task LogAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            // Mask sensitive data in metadata
            var maskedMetadata = auditEvent.Metadata != null
                ? _dataMasker.MaskSensitiveData(auditEvent.Metadata)
                : null;

            var logLevel = auditEvent.IsSuccess ? LogLevel.Information : LogLevel.Warning;

            // Get i18n message based on event type
            var i18nKey = auditEvent.EventType switch
            {
                "HTTP_REQUEST" => I18nKeys.Interceptor.Audit.Format.HttpRequestSuccess,
                "SERVICE_CALL" => I18nKeys.Interceptor.Audit.Format.ServiceCallSuccess,
                "JOB_EXECUTION" => I18nKeys.Interceptor.Audit.Format.JobComplete,
                "HOSTED_SERVICE" => I18nKeys.Interceptor.Audit.Format.JobComplete,
                _ => I18nKeys.Interceptor.Audit.HttpRequest
            };

            if (!auditEvent.IsSuccess)
            {
                i18nKey = auditEvent.EventType switch
                {
                    "HTTP_REQUEST" => I18nKeys.Interceptor.Audit.Format.HttpRequestFailed,
                    "SERVICE_CALL" => I18nKeys.Interceptor.Audit.Format.ServiceCallFailed,
                    "JOB_EXECUTION" => I18nKeys.Interceptor.Audit.Format.JobFailed,
                    _ => I18nKeys.Interceptor.Audit.HttpRequest
                };
            }

            var logMessage = _i18nService.GetLogMessage(
                i18nKey,
                auditEvent.EventType,
                auditEvent.Action,
                auditEvent.UserId ?? "Anonymous",
                auditEvent.CorrelationId ?? "N/A",
                auditEvent.ResourcePath ?? auditEvent.MethodName ?? "N/A",
                auditEvent.ExecutionTimeMs ?? 0,
                auditEvent.IsSuccess
            );

            _logger.Log(logLevel, logMessage);

            // Log metadata if present (masked)
            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[AUDIT_METADATA] {EventType} | {Metadata}",
                    auditEvent.EventType,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata)
                );
            }

            // Log error if present
            if (!auditEvent.IsSuccess && !string.IsNullOrEmpty(auditEvent.ErrorMessage))
            {
                _logger.LogWarning(
                    "[AUDIT_ERROR] {EventType} | {ErrorMessage} | ErrorCode: {ErrorCode}",
                    auditEvent.EventType,
                    auditEvent.ErrorMessage,
                    auditEvent.ErrorCode ?? "UNKNOWN"
                );
            }
        }
        catch (Exception ex)
        {
            // Don't let audit logging break the application
            _logger.LogError(ex, "Failed to log audit event");
        }

        return Task.CompletedTask;
    }
}
