using CanPany.Application.Interfaces.Interceptors;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Infrastructure.Interceptors;

/// <summary>
/// Simplified audit logger for Worker service (no I18N dependency)
/// </summary>
public class WorkerAuditLogger : IAuditLogger
{
    private readonly ILogger<WorkerAuditLogger> _logger;
    private readonly IDataMasker _dataMasker;

    public WorkerAuditLogger(
        ILogger<WorkerAuditLogger> logger,
        IDataMasker dataMasker)
    {
        _logger = logger;
        _dataMasker = dataMasker;
    }

    public Task LogAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var maskedMetadata = auditEvent.Metadata != null
                ? _dataMasker.MaskSensitiveData(auditEvent.Metadata)
                : null;

            var logLevel = auditEvent.IsSuccess ? LogLevel.Information : LogLevel.Warning;

            var statusTag = auditEvent.IsSuccess ? "SUCCESS" : "FAILED";

            _logger.Log(
                logLevel,
                "[AUDIT_{Status}] {EventType} | Action: {Action} | Resource: {Resource} | Duration: {Duration}ms | Time: {Timestamp}",
                statusTag,
                auditEvent.EventType,
                auditEvent.Action,
                auditEvent.ResourcePath ?? auditEvent.MethodName ?? "N/A",
                auditEvent.ExecutionTimeMs ?? 0,
                auditEvent.Timestamp
            );

            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[AUDIT_METADATA] {EventType} | {Metadata}",
                    auditEvent.EventType,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata)
                );
            }

            if (!auditEvent.IsSuccess && !string.IsNullOrEmpty(auditEvent.ErrorMessage))
            {
                _logger.LogWarning(
                    "[AUDIT_ERROR] {EventType} | Error: {ErrorMessage} | Code: {ErrorCode}",
                    auditEvent.EventType,
                    auditEvent.ErrorMessage,
                    auditEvent.ErrorCode ?? "UNKNOWN"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event");
        }

        return Task.CompletedTask;
    }
}
