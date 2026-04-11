using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Audit logger implementation — writes to ILogger AND persists to MongoDB via IAuditLogRepository.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IDataMasker _dataMasker;
    private readonly II18nService _i18nService;
    private readonly IAuditLogRepository _auditLogRepo;

    public AuditLogger(
        ILogger<AuditLogger> logger,
        IDataMasker dataMasker,
        II18nService i18nService,
        IAuditLogRepository auditLogRepo)
    {
        _logger = logger;
        _dataMasker = dataMasker;
        _i18nService = i18nService;
        _auditLogRepo = auditLogRepo;
    }

    public async Task LogAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            // ── 1. Mask sensitive data ──────────────────────────────────────
            var maskedMetadata = auditEvent.Metadata != null
                ? _dataMasker.MaskSensitiveData(auditEvent.Metadata)
                : null;

            // ── 2. Console / structured-log ────────────────────────────────
            var logLevel = auditEvent.IsSuccess ? LogLevel.Information : LogLevel.Warning;

            var i18nKey = auditEvent.IsSuccess
                ? auditEvent.EventType switch
                {
                    "HTTP_REQUEST"    => I18nKeys.Interceptor.Audit.Format.HttpRequestSuccess,
                    "SERVICE_CALL"   => I18nKeys.Interceptor.Audit.Format.ServiceCallSuccess,
                    "JOB_EXECUTION"  => I18nKeys.Interceptor.Audit.Format.JobComplete,
                    "HOSTED_SERVICE" => I18nKeys.Interceptor.Audit.Format.JobComplete,
                    _                => I18nKeys.Interceptor.Audit.HttpRequest
                }
                : auditEvent.EventType switch
                {
                    "HTTP_REQUEST"   => I18nKeys.Interceptor.Audit.Format.HttpRequestFailed,
                    "SERVICE_CALL"  => I18nKeys.Interceptor.Audit.Format.ServiceCallFailed,
                    "JOB_EXECUTION" => I18nKeys.Interceptor.Audit.Format.JobFailed,
                    _               => I18nKeys.Interceptor.Audit.HttpRequest
                };

            var logMessage = _i18nService.GetLogMessage(
                i18nKey,
                auditEvent.EventType,
                auditEvent.Action,
                auditEvent.UserId ?? "Anonymous",
                auditEvent.CorrelationId ?? "N/A",
                auditEvent.ResourcePath ?? auditEvent.MethodName ?? "N/A",
                auditEvent.ExecutionTimeMs ?? 0,
                auditEvent.IsSuccess,
                auditEvent.ErrorMessage ?? string.Empty);

            _logger.Log(logLevel, logMessage);

            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[AUDIT_METADATA] {EventType} | {Metadata}",
                    auditEvent.EventType,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata));
            }

            if (!auditEvent.IsSuccess && !string.IsNullOrEmpty(auditEvent.ErrorMessage))
            {
                _logger.LogWarning(
                    "[AUDIT_ERROR] {EventType} | {ErrorMessage} | ErrorCode: {ErrorCode}",
                    auditEvent.EventType,
                    auditEvent.ErrorMessage,
                    auditEvent.ErrorCode ?? "UNKNOWN");
            }

            // ── 3. Persist to MongoDB ───────────────────────────────────────
            if (auditEvent.EventType != "HTTP_REQUEST")
            {
                return;
            }
            
            // Extract IpAddress / UserAgent from metadata if available
            var ip       = maskedMetadata?.TryGetValue("IpAddress",  out var ipVal)  == true ? ipVal?.ToString()  : null;
            var ua       = maskedMetadata?.TryGetValue("UserAgent",  out var uaVal)  == true ? uaVal?.ToString()  : null;
            var qs       = maskedMetadata?.TryGetValue("QueryString",out var qsVal)  == true ? qsVal?.ToString()  : null;
            var method   = maskedMetadata?.TryGetValue("HttpMethod", out var mVal)   == true ? mVal?.ToString()   : auditEvent.Action;
            var name     = maskedMetadata?.TryGetValue("UserFullName",out var nVal)  == true ? nVal?.ToString()   : null;
            var status   = maskedMetadata?.TryGetValue("StatusCode", out var scVal)  == true
                               && int.TryParse(scVal?.ToString(), out var sc) ? (int?)sc : null;

            var entry = new AuditLog
            {
                UserId              = auditEvent.UserId,
                UserEmail           = name,
                Action              = auditEvent.Action,
                HttpMethod          = method ?? auditEvent.Action,
                RequestPath         = auditEvent.ResourcePath ?? auditEvent.MethodName ?? string.Empty,
                Endpoint            = auditEvent.ResourcePath ?? auditEvent.MethodName ?? string.Empty,
                QueryString         = qs,
                ResponseStatusCode  = status,
                IpAddress           = ip,
                UserAgent           = ua,
                Duration            = auditEvent.ExecutionTimeMs,
                ErrorMessage        = auditEvent.IsSuccess ? null : auditEvent.ErrorMessage,
                CreatedAt           = auditEvent.Timestamp,
            };

            await _auditLogRepo.AddAsync(entry);
        }
        catch (Exception ex)
        {
            // Never let audit logging break the request pipeline
            _logger.LogError(ex, "Failed to log audit event");
        }
    }
}
