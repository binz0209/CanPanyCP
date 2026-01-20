namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Exception capture interface
/// </summary>
public interface IExceptionCapture
{
    /// <summary>
    /// Capture exception with context
    /// </summary>
    Task CaptureExceptionAsync(Exception exception, ExceptionContext context);
}

/// <summary>
/// Exception context model
/// </summary>
public class ExceptionContext
{
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
