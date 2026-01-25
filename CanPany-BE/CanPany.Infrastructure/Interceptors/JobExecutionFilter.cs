using CanPany.Application.Interfaces.Interceptors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Hangfire.Server;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Background job execution filter for Hangfire
/// Note: Requires Hangfire package. If not using Hangfire, use HostedServiceInterceptor instead.
/// This class is provided as a template - uncomment and add Hangfire package reference when needed.
/// </summary>
public class JobExecutionFilter : IServerFilter
{
    private readonly IAuditLogger _auditLogger;
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IExceptionCapture _exceptionCapture;
    private readonly ILogger<JobExecutionFilter> _logger;

    public JobExecutionFilter(
        IAuditLogger auditLogger,
        IPerformanceMonitor performanceMonitor,
        IExceptionCapture exceptionCapture,
        ILogger<JobExecutionFilter> logger)
    {
        _auditLogger = auditLogger;
        _performanceMonitor = performanceMonitor;
        _exceptionCapture = exceptionCapture;
        _logger = logger;
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        var jobName = filterContext.BackgroundJob.Job.Method.Name;
        var jobId = filterContext.BackgroundJob.Id;
        var correlationId = Guid.NewGuid().ToString();

        filterContext.Items["CorrelationId"] = correlationId;
        filterContext.Items["StartTime"] = DateTime.UtcNow;

        _logger.LogInformation(
            "[JOB_START] {JobName} | JobId: {JobId} | CorrelationId: {CorrelationId}",
            jobName,
            jobId,
            correlationId
        );

        _auditLogger.LogAuditEventAsync(new AuditEvent
        {
            EventType = "JOB_EXECUTION",
            Action = jobName,
            CorrelationId = correlationId,
            MethodName = jobName,
            Metadata = new Dictionary<string, object?>
            {
                ["JobId"] = jobId,
                ["JobType"] = filterContext.BackgroundJob.Job.Type.Name
            }
        }).GetAwaiter().GetResult();
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobName = filterContext.BackgroundJob.Job.Method.Name;
        var jobId = filterContext.BackgroundJob.Id;
        var correlationId = filterContext.Items.TryGetValue("CorrelationId", out var corrId) ? corrId?.ToString() : null;
        var startTime = filterContext.Items.TryGetValue("StartTime", out var start) && start is DateTime dt ? dt : DateTime.UtcNow;
        var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

        _logger.LogInformation(
            "[JOB_COMPLETE] {JobName} | JobId: {JobId} | CorrelationId: {CorrelationId} | Duration: {ExecutionTime}ms",
            jobName,
            jobId,
            correlationId,
            executionTime
        );

        _auditLogger.LogAuditEventAsync(new AuditEvent
        {
            EventType = "JOB_EXECUTION",
            Action = jobName,
            CorrelationId = correlationId,
            MethodName = jobName,
            ExecutionTimeMs = (long)executionTime,
            IsSuccess = true,
            Metadata = new Dictionary<string, object?>
            {
                ["JobId"] = jobId,
                ["JobType"] = filterContext.BackgroundJob.Job.Type.Name
            }
        }).GetAwaiter().GetResult();

        _performanceMonitor.RecordExecutionTime(jobName, (long)executionTime, new Dictionary<string, object?>
        {
            ["JobId"] = jobId,
            ["JobType"] = filterContext.BackgroundJob.Job.Type.Name
        });
    }

    public void OnServerException(ServerExceptionContext filterContext)
    {
        var jobName = filterContext.BackgroundJob.Job.Method.Name;
        var jobId = filterContext.BackgroundJob.Id;
        var correlationId = filterContext.Items.TryGetValue("CorrelationId", out var corrId) ? corrId?.ToString() : null;
        var exception = filterContext.Exception;

        _logger.LogError(
            exception,
            "[JOB_FAILED] {JobName} | JobId: {JobId} | CorrelationId: {CorrelationId}",
            jobName,
            jobId,
            correlationId
        );

        _exceptionCapture.CaptureExceptionAsync(exception, new ExceptionContext
        {
            CorrelationId = correlationId,
            ServiceName = filterContext.BackgroundJob.Job.Type.Name,
            MethodName = jobName,
            Metadata = new Dictionary<string, object?>
            {
                ["JobId"] = jobId
            }
        }).GetAwaiter().GetResult();

        _auditLogger.LogAuditEventAsync(new AuditEvent
        {
            EventType = "JOB_EXECUTION",
            Action = jobName,
            CorrelationId = correlationId,
            MethodName = jobName,
            IsSuccess = false,
            ErrorMessage = exception.Message,
            ErrorCode = exception.GetType().Name,
            Metadata = new Dictionary<string, object?>
            {
                ["JobId"] = jobId,
                ["JobType"] = filterContext.BackgroundJob.Job.Type.Name
            }
        }).GetAwaiter().GetResult();
    }
}


