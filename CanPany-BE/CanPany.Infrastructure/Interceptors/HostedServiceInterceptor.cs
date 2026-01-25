using CanPany.Application.Interfaces.Interceptors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Hosted service interceptor for background workers
/// Provides audit, performance monitoring, and exception tracking without HttpContext
/// </summary>
public class HostedServiceInterceptor : IHostedServiceInterceptor
{
    private readonly IAuditLogger _auditLogger;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IExceptionCapture _exceptionCapture;
    private readonly ILogger<HostedServiceInterceptor> _logger;

    public HostedServiceInterceptor(
        IAuditLogger auditLogger,
        IPerformanceMonitor performanceMonitor,
        IExceptionCapture exceptionCapture,
        ILogger<HostedServiceInterceptor> logger)
    {
        _auditLogger = auditLogger;
        _performanceMonitor = performanceMonitor;
        _exceptionCapture = exceptionCapture;
        _logger = logger;
    }

    public async Task<T> ExecuteWithInterceptionAsync<T>(
        string serviceName,
        string methodName,
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationName = $"{serviceName}.{methodName}";

        try
        {
            _logger.LogDebug("[INTERCEPT_START] {Operation}", operationName);

            var result = await operation();

            stopwatch.Stop();

            _performanceMonitor.RecordExecutionTime(
                operationName,
                stopwatch.ElapsedMilliseconds,
                new Dictionary<string, object?>
                {
                    ["ServiceName"] = serviceName,
                    ["MethodName"] = methodName,
                    ["ResultType"] = typeof(T).Name
                });

            await _auditLogger.LogAuditEventAsync(new AuditEvent
            {
                EventType = "BackgroundJob",
                Action = methodName,
                ResourcePath = serviceName,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object?>
                {
                    ["ResultType"] = typeof(T).Name
                }
            });

            _logger.LogDebug("[INTERCEPT_SUCCESS] {Operation} completed in {Duration}ms", 
                operationName, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await _exceptionCapture.CaptureExceptionAsync(ex, new ExceptionContext
            {
                ServiceName = serviceName,
                MethodName = methodName,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object?>
                {
                    ["OperationName"] = operationName,
                    ["DurationMs"] = stopwatch.ElapsedMilliseconds
                }
            });

            await _auditLogger.LogAuditEventAsync(new AuditEvent
            {
                EventType = "BackgroundJob",
                Action = methodName,
                ResourcePath = serviceName,
                Timestamp = DateTime.UtcNow,
                IsSuccess = false,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object?>
                {
                    ["ErrorMessage"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name
                }
            });

            _logger.LogError(ex, "[INTERCEPT_ERROR] {Operation} failed after {Duration}ms", 
                operationName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task ExecuteWithInterceptionAsync(
        string serviceName,
        string methodName,
        Func<Task> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationName = $"{serviceName}.{methodName}";

        try
        {
            _logger.LogDebug("[INTERCEPT_START] {Operation}", operationName);

            await operation();

            stopwatch.Stop();

            _performanceMonitor.RecordExecutionTime(
                operationName,
                stopwatch.ElapsedMilliseconds,
                new Dictionary<string, object?>
                {
                    ["ServiceName"] = serviceName,
                    ["MethodName"] = methodName
                });

            await _auditLogger.LogAuditEventAsync(new AuditEvent
            {
                EventType = "BackgroundJob",
                Action = methodName,
                ResourcePath = serviceName,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds
            });

            _logger.LogDebug("[INTERCEPT_SUCCESS] {Operation} completed in {Duration}ms", 
                operationName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await _exceptionCapture.CaptureExceptionAsync(ex, new ExceptionContext
            {
                ServiceName = serviceName,
                MethodName = methodName,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object?>
                {
                    ["OperationName"] = operationName,
                    ["DurationMs"] = stopwatch.ElapsedMilliseconds
                }
            });

            await _auditLogger.LogAuditEventAsync(new AuditEvent
            {
                EventType = "BackgroundJob",
                Action = methodName,
                ResourcePath = serviceName,
                Timestamp = DateTime.UtcNow,
                IsSuccess = false,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                Metadata = new Dictionary<string, object?>
                {
                    ["ErrorMessage"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name
                }
            });

            _logger.LogError(ex, "[INTERCEPT_ERROR] {Operation} failed after {Duration}ms", 
                operationName, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
