using CanPany.Application.Interfaces.Interceptors;
using CanPany.Infrastructure.Interceptors;
using CanPany.Worker.Handlers;
using CanPany.Worker.Handlers.Samples;
using CanPany.Worker.Infrastructure.Interceptors;
using CanPany.Worker.Infrastructure.Queue;
using CanPany.Worker.Infrastructure.Registry;
using CanPany.Worker.Infrastructure.Progress;
using CanPany.Worker.Workers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CanPany.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Redis Connection
        var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse(redisConnection);
            config.AbortOnConnectFail = false;
            config.ConnectTimeout = 10000;
            config.SyncTimeout = 10000;
            return ConnectionMultiplexer.Connect(config);
        });

        // Job Queue
        builder.Services.AddSingleton<IJobQueue, RedisJobQueue>();
        
        // Job Producer (for API integration)
        builder.Services.AddSingleton<IJobProducer, RedisJobProducer>();

        // Job Progress Tracker
        builder.Services.AddSingleton<IJobProgressTracker, RedisJobProgressTracker>();

        // Interceptors (simplified versions for Worker - no I18N dependency)
        builder.Services.AddSingleton<IDataMasker, WorkerDataMasker>();
        builder.Services.AddSingleton<IAuditLogger, WorkerAuditLogger>();
        builder.Services.AddSingleton<IPerformanceMonitor, WorkerPerformanceMonitor>();
        builder.Services.AddSingleton<IExceptionCapture, WorkerExceptionCapture>();
        builder.Services.AddSingleton<IHostedServiceInterceptor, HostedServiceInterceptor>();

        // Register Job Handlers
        builder.Services.AddSingleton<IJobHandler, SendEmailJobHandler>();
        builder.Services.AddSingleton<IJobHandler, AIMatchingJobHandler>();
        builder.Services.AddSingleton<IJobHandler, GenerateReportJobHandler>();

        // Job Handler Registry (register and populate)
        builder.Services.AddSingleton<JobHandlerRegistry>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<JobHandlerRegistry>>();
            var registry = new JobHandlerRegistry(logger);
            var handlers = sp.GetServices<IJobHandler>();
            
            foreach (var handler in handlers)
            {
                registry.Register(handler);
            }

            return registry;
        });

        // Background Worker
        builder.Services.AddHostedService<JobProcessorWorker>();

        // Health Checks (optional)
        builder.Services.AddHealthChecks()
            .AddCheck<WorkerHealthCheck>("worker-health");

        var host = builder.Build();

        // Log startup info
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("=================================================");
        logger.LogInformation("  CanPany Background Job Worker");
        logger.LogInformation("  Environment: {Environment}", builder.Environment.EnvironmentName);
        logger.LogInformation("  Redis: {Redis}", redisConnection);
        logger.LogInformation("=================================================");

        host.Run();
    }
}

/// <summary>
/// Health check for worker service
/// </summary>
public class WorkerHealthCheck : IHealthCheck
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<WorkerHealthCheck> _logger;

    public WorkerHealthCheck(IJobQueue jobQueue, ILogger<WorkerHealthCheck> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queueDepth = await _jobQueue.GetQueueDepthAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["QueueDepth"] = queueDepth,
                ["Status"] = "Healthy"
            };

            return HealthCheckResult.Healthy("Worker is running", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HEALTH_CHECK_FAILED]");
            return HealthCheckResult.Unhealthy("Worker health check failed", ex);
        }
    }
}
