namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Audit logging interface for cross-cutting concerns
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log audit event with context
    /// </summary>
    Task LogAuditEventAsync(AuditEvent auditEvent);
}

/// <summary>
/// Audit event model
/// </summary>
public class AuditEvent
{
    public string EventType { get; set; } = string.Empty; // "HTTP_REQUEST", "SERVICE_CALL", "JOB_EXECUTION"
    public string Action { get; set; } = string.Empty; // "GET", "POST", "CreateProfile", "SyncJob"
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ResourcePath { get; set; }
    public string? MethodName { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long? ExecutionTimeMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
}
