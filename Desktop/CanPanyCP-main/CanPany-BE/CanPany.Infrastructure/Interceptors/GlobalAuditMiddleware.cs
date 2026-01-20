using CanPany.Application.Interfaces.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Global audit middleware for HTTP requests
/// </summary>
public class GlobalAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IDataMasker _dataMasker;
    private readonly ILogger<GlobalAuditMiddleware> _logger;

    public GlobalAuditMiddleware(
        RequestDelegate next,
        IServiceScopeFactory serviceScopeFactory,
        IDataMasker dataMasker,
        ILogger<GlobalAuditMiddleware> logger)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
        _dataMasker = dataMasker;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetOrCreateCorrelationId(context);
        var userId = GetUserId(context);
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var httpMethod = context.Request.Method;

        // Skip health checks and swagger
        if (ShouldSkipAudit(requestPath))
        {
            await _next(context);
            return;
        }

        // Create scope for scoped services
        using var scope = _serviceScopeFactory.CreateScope();
        var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
        var performanceMonitor = scope.ServiceProvider.GetRequiredService<IPerformanceMonitor>();
        var securityTracker = scope.ServiceProvider.GetRequiredService<ISecurityEventTracker>();
        var exceptionCapture = scope.ServiceProvider.GetRequiredService<IExceptionCapture>();

        // Track security event for authentication
        if (IsAuthenticationEndpoint(requestPath))
        {
            await TrackAuthenticationEvent(context, userId, correlationId, securityTracker);
        }

        // Capture request metadata (masked)
        var requestMetadata = CaptureRequestMetadata(context);

        try
        {
            // Execute next middleware
            await _next(context);

            stopwatch.Stop();

            // Log audit event
            await auditLogger.LogAuditEventAsync(new AuditEvent
            {
                EventType = "HTTP_REQUEST",
                Action = httpMethod,
                UserId = userId,
                CorrelationId = correlationId,
                ResourcePath = requestPath,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = context.Response.StatusCode < 400,
                Metadata = requestMetadata
            });

            // Track security event for authorization failures
            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                await securityTracker.TrackSecurityEventAsync(new SecurityEvent
                {
                    EventType = "AUTHORIZATION",
                    Severity = "WARNING",
                    UserId = userId,
                    ResourcePath = requestPath,
                    Action = httpMethod,
                    IpAddress = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    IsSuccess = false,
                    FailureReason = $"HTTP {context.Response.StatusCode}"
                });
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Capture exception
            await exceptionCapture.CaptureExceptionAsync(ex, new ExceptionContext
            {
                UserId = userId,
                CorrelationId = correlationId,
                RequestPath = requestPath,
                HttpMethod = httpMethod,
                Metadata = requestMetadata
            });

            // Track security event for exceptions
            await securityTracker.TrackSecurityEventAsync(new SecurityEvent
            {
                EventType = "EXCEPTION",
                Severity = "ERROR",
                UserId = userId,
                ResourcePath = requestPath,
                Action = httpMethod,
                IpAddress = GetClientIpAddress(context),
                IsSuccess = false,
                FailureReason = ex.Message
            });

            // Re-throw to let error handling middleware handle it
            throw;
        }
    }

    private static bool ShouldSkipAudit(string path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/healthz",
            "/swagger",
            "/favicon.ico",
            "/_vs"
        };

        return skipPaths.Any(skip => path.StartsWith(skip, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAuthenticationEndpoint(string path)
    {
        return path.Contains("/auth/", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("/login", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-Id"] = newCorrelationId;
        context.Response.Headers["X-Correlation-Id"] = newCorrelationId;
        return newCorrelationId;
    }

    private static string? GetUserId(HttpContext context)
    {
        return context.User?.FindFirst("sub")?.Value ??
               context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    private Dictionary<string, object?> CaptureRequestMetadata(HttpContext context)
    {
        var metadata = new Dictionary<string, object?>
        {
            ["HttpMethod"] = context.Request.Method,
            ["Path"] = context.Request.Path.Value,
            ["QueryString"] = context.Request.QueryString.Value,
            ["StatusCode"] = context.Response.StatusCode,
            ["ContentType"] = context.Request.ContentType,
            ["ContentLength"] = context.Request.ContentLength,
            ["IpAddress"] = GetClientIpAddress(context),
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString()
        };

        // Capture headers (masked)
        var headers = new Dictionary<string, string>();
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                headers[header.Key] = "***MASKED***";
            }
            else
            {
                headers[header.Key] = header.Value.ToString();
            }
        }
        metadata["Headers"] = headers;

        return metadata;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
               context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
               context.Connection.RemoteIpAddress?.ToString() ??
               "Unknown";
    }

    private async Task TrackAuthenticationEvent(HttpContext context, string? userId, string correlationId, ISecurityEventTracker securityTracker)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var isLogin = requestPath.Contains("/login", StringComparison.OrdinalIgnoreCase);
        var isSuccess = context.Response.StatusCode == 200 || context.Response.StatusCode == 201;

        await securityTracker.TrackSecurityEventAsync(new SecurityEvent
        {
            EventType = "AUTHENTICATION",
            Severity = isSuccess ? "INFO" : "WARNING",
            UserId = userId,
            ResourcePath = requestPath,
            Action = context.Request.Method,
            IpAddress = GetClientIpAddress(context),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            IsSuccess = isSuccess,
            FailureReason = isSuccess ? null : $"HTTP {context.Response.StatusCode}"
        });
    }
}
