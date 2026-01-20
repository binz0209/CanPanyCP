using CanPany.Application.Interfaces.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Service interceptor decorator - wraps service calls with interceptors
/// </summary>
public class ServiceInterceptor<T> : DispatchProxy where T : class
{
    private T? _target;
    private IAuditLogger? _auditLogger;
    private IPerformanceMonitor? _performanceMonitor;
    private IExceptionCapture? _exceptionCapture;
    private IDataMasker? _dataMasker;
    private IHttpContextAccessor? _httpContextAccessor;
    private ILogger<ServiceInterceptor<T>>? _logger;

    public static T Create(
        T target,
        IAuditLogger auditLogger,
        IPerformanceMonitor performanceMonitor,
        IExceptionCapture exceptionCapture,
        IDataMasker dataMasker,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServiceInterceptor<T>> logger)
    {
        var proxy = Create<T, ServiceInterceptor<T>>() as ServiceInterceptor<T>;
        
        if (proxy == null)
            throw new InvalidOperationException($"Failed to create proxy for {typeof(T).Name}");

        proxy._target = target;
        proxy._auditLogger = auditLogger;
        proxy._performanceMonitor = performanceMonitor;
        proxy._exceptionCapture = exceptionCapture;
        proxy._dataMasker = dataMasker;
        proxy._httpContextAccessor = httpContextAccessor;
        proxy._logger = logger;

        return (T)(object)proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null || _target == null)
            return null;

        var methodName = $"{typeof(T).Name}.{targetMethod.Name}";
        var stopwatch = Stopwatch.StartNew();
        var userId = GetUserId();
        var correlationId = GetCorrelationId();

        // Mask arguments
        var maskedArgs = MaskArguments(args);

        try
        {
            // Execute method
            var result = targetMethod.Invoke(_target, args);
            
            stopwatch.Stop();

            // Log audit event
            _auditLogger?.LogAuditEventAsync(new AuditEvent
            {
                EventType = "SERVICE_CALL",
                Action = methodName,
                UserId = userId,
                CorrelationId = correlationId,
                MethodName = methodName,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = true,
                Metadata = new Dictionary<string, object?>
                {
                    ["Arguments"] = maskedArgs,
                    ["ReturnType"] = targetMethod.ReturnType.Name
                }
            }).GetAwaiter().GetResult();

            // Record performance
            _performanceMonitor?.RecordExecutionTime(methodName, stopwatch.ElapsedMilliseconds, new Dictionary<string, object?>
            {
                ["Service"] = typeof(T).Name,
                ["Method"] = targetMethod.Name
            });

            // Handle async methods
            if (result is Task task)
            {
                // For Task<T>, return the task as-is (DispatchProxy handles it)
                return task;
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Unwrap TargetInvocationException (from reflection)
            var actualException = ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                ? tie.InnerException
                : ex;
            
            HandleException(actualException, methodName, userId, correlationId, maskedArgs);

            // Re-throw original exception to maintain behavior
            throw actualException;
        }
    }

    private void HandleException(Exception ex, string methodName, string? userId, string? correlationId, Dictionary<string, object?>? maskedArgs)
    {
        _exceptionCapture?.CaptureExceptionAsync(ex, new ExceptionContext
        {
            UserId = userId,
            CorrelationId = correlationId,
            ServiceName = typeof(T).Name,
            MethodName = methodName,
            Metadata = maskedArgs != null ? new Dictionary<string, object?>(maskedArgs) { ["Exception"] = ex.GetType().Name } : null
        }).GetAwaiter().GetResult();

        _auditLogger?.LogAuditEventAsync(new AuditEvent
        {
            EventType = "SERVICE_CALL",
            Action = methodName,
            UserId = userId,
            CorrelationId = correlationId,
            MethodName = methodName,
            IsSuccess = false,
            ErrorMessage = ex.Message,
            ErrorCode = ex.GetType().Name,
            Metadata = maskedArgs
        }).GetAwaiter().GetResult();
    }

    private Dictionary<string, object?>? MaskArguments(object?[]? args)
    {
        if (args == null || args.Length == 0)
            return null;

        var masked = new Dictionary<string, object?>();
        for (int i = 0; i < args.Length; i++)
        {
            var key = $"arg{i}";
            masked[key] = _dataMasker?.MaskSensitiveData(args[i]);
        }
        return masked;
    }

    private string? GetUserId()
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value ??
               _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetCorrelationId()
    {
        return _httpContextAccessor?.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault();
    }
}

/// <summary>
/// Service interceptor factory
/// </summary>
public static class ServiceInterceptorFactory
{
    public static T CreateInterceptor<T>(
        T target,
        IServiceProvider serviceProvider) where T : class
    {
        var auditLogger = serviceProvider.GetRequiredService<IAuditLogger>();
        var performanceMonitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();
        var exceptionCapture = serviceProvider.GetRequiredService<IExceptionCapture>();
        var dataMasker = serviceProvider.GetRequiredService<IDataMasker>();
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var logger = serviceProvider.GetRequiredService<ILogger<ServiceInterceptor<T>>>();

        return ServiceInterceptor<T>.Create(
            target,
            auditLogger,
            performanceMonitor,
            exceptionCapture,
            dataMasker,
            httpContextAccessor,
            logger);
    }
}
