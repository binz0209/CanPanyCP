namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Security event tracking interface
/// </summary>
public interface ISecurityEventTracker
{
    /// <summary>
    /// Track security event
    /// </summary>
    Task TrackSecurityEventAsync(SecurityEvent securityEvent);
}

/// <summary>
/// Security event model
/// </summary>
public class SecurityEvent
{
    public string EventType { get; set; } = string.Empty; // "AUTHENTICATION", "AUTHORIZATION", "DATA_ACCESS", "TOKEN_ACCESS"
    public string Severity { get; set; } = "INFO"; // "INFO", "WARNING", "ERROR", "CRITICAL"
    public string? UserId { get; set; }
    public string? ResourcePath { get; set; }
    public string? Action { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
    public string? FailureReason { get; set; }
}
