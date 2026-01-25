using CanPany.Application.Interfaces.Interceptors;
using CanPany.Worker.Infrastructure.Queue;
using CanPany.Worker.Infrastructure.Registry;
using CanPany.Worker.Infrastructure.Progress;
using CanPany.Worker.Models;
using CanPany.Worker.Handlers;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;

namespace CanPany.Worker.Workers;

/// <summary>
/// Concurrent job processor worker
/// Processes jobs from Redis queue with configurable concurrency
/// </summary>
public class JobProcessorWorker : BackgroundService
{
    private readonly IJobQueue _jobQueue;
    private readonly JobHandlerRegistry _handlerRegistry;
    private readonly IJobProgressTracker _progressTracker;
    private readonly IHostedServiceInterceptor _interceptor;
    private readonly ILogger<JobProcessorWorker> _logger;
    private readonly IConfiguration _configuration;
    
    private readonly int _maxConcurrentJobs;
    private readonly int _pollingIntervalSeconds;
    private readonly int _maxRetries;
    
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentDictionary<string, Task> _runningJobs = new();
    private readonly ResiliencePipeline _resiliencePipeline;

    public JobProcessorWorker(
        IJobQueue jobQueue,
        JobHandlerRegistry handlerRegistry,
        IJobProgressTracker progressTracker,
        IHostedServiceInterceptor interceptor,
        ILogger<JobProcessorWorker> logger,
        IConfiguration configuration)
    {
        _jobQueue = jobQueue;
        _handlerRegistry = handlerRegistry;
        _progressTracker = progressTracker;
        _interceptor = interceptor;
        _logger = logger;
        _configuration = configuration;

        _maxConcurrentJobs = configuration.GetValue<int>("Worker:MaxConcurrentJobs", 10);
        _pollingIntervalSeconds = configuration.GetValue<int>("Worker:PollingIntervalSeconds", 5);
        _maxRetries = configuration.GetValue<int>("Worker:MaxRetries", 3);

        _semaphore = new SemaphoreSlim(_maxConcurrentJobs);

        // Build resilience pipeline (retry + circuit breaker)
        _resiliencePipeline = BuildResiliencePipeline();

        _logger.LogInformation(
            "[WORKER_CONFIG] MaxConcurrent: {MaxConcurrent} | PollingInterval: {PollingInterval}s | MaxRetries: {MaxRetries}",
            _maxConcurrentJobs,
            _pollingIntervalSeconds,
            _maxRetries
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[WORKER_STARTED] JobProcessorWorker is now running");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for available slot
                await _semaphore.WaitAsync(stoppingToken);

                try
                {
                    // Dequeue job (non-blocking with timeout)
                    var job = await _jobQueue.DequeueAsync(stoppingToken);

                    if (job == null)
                    {
                        // No jobs available - release semaphore and wait
                        _semaphore.Release();
                        await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalSeconds), stoppingToken);
                        continue;
                    }

                    // Process job concurrently
                    var jobTask = ProcessJobAsync(job, stoppingToken);
                    
                    _runningJobs[job.JobId] = jobTask;

                    // Don't await - let it run concurrently
                    _ = jobTask.ContinueWith(t =>
                    {
                        _runningJobs.TryRemove(job.JobId, out _);
                        _semaphore.Release();

                        if (t.IsFaulted)
                        {
                            _logger.LogError(t.Exception, "[JOB_TASK_FAULTED] JobId: {JobId}", job.JobId);
                        }
                    }, TaskScheduler.Default);
                }
                catch (Exception ex)
                {
                    _semaphore.Release();
                    _logger.LogError(ex, "[WORKER_LOOP_ERROR]");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[WORKER_STOPPING] Cancellation requested");
                break;
            }
        }

        // Wait for all running jobs to complete
        _logger.LogInformation("[WORKER_SHUTDOWN] Waiting for {Count} jobs to complete", _runningJobs.Count);
        await Task.WhenAll(_runningJobs.Values);
        
        _logger.LogInformation("[WORKER_STOPPED] All jobs completed");
    }

    private async Task ProcessJobAsync(JobMessage job, CancellationToken cancellationToken)
    {
        var handler = _handlerRegistry.GetHandler(job.I18nKey);

        if (handler == null)
        {
            _logger.LogWarning(
                "[NO_HANDLER] JobId: {JobId} | I18nKey: {I18nKey} - Moving to DLQ",
                job.JobId,
                job.I18nKey
            );

            await _progressTracker.MarkAsFailedAsync(job.JobId, "No handler found", cancellationToken);
            await _jobQueue.MoveToDeadLetterQueueAsync(job, "No handler found", cancellationToken);
            await _jobQueue.AcknowledgeAsync(job, cancellationToken);
            return;
        }

        // Inject progress tracker into handler
        if (handler is BaseJobHandler baseHandler)
        {
            baseHandler.SetProgressTracker(_progressTracker);
        }

        // Mark job as running
        await _progressTracker.MarkAsRunningAsync(job.JobId, cancellationToken);

        try
        {
            // Execute with interceptor wrapper (audit, performance, exception tracking)
            var result = await _interceptor.ExecuteWithInterceptionAsync(
                serviceName: "JobProcessor",
                methodName: job.I18nKey,
                operation: async () =>
                {
                    // Execute with resilience pipeline (retry + circuit breaker)
                    return await _resiliencePipeline.ExecuteAsync(
                        async ct => await handler.ExecuteAsync(job, ct),
                        cancellationToken
                    );
                }
            );

            if (result.Success)
            {
                _logger.LogInformation(
                    "[JOB_SUCCESS] JobId: {JobId} | I18nKey: {I18nKey} | Metadata: {Metadata}",
                    job.JobId,
                    job.I18nKey,
                    System.Text.Json.JsonSerializer.Serialize(result.Metadata)
                );

                await _progressTracker.MarkAsCompletedAsync(job.JobId, result.Metadata, cancellationToken);
                await _jobQueue.AcknowledgeAsync(job, cancellationToken);
            }
            else
            {
                await HandleJobFailure(job, result.ErrorMessage ?? "Unknown error", cancellationToken);
            }
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(
                "[CIRCUIT_BREAKER_OPEN] JobId: {JobId} | I18nKey: {I18nKey} - Requeuing",
                job.JobId,
                job.I18nKey
            );

            await _progressTracker.MarkAsRetryingAsync(job.JobId, cancellationToken);
            // Circuit breaker open - requeue with delay
            await _jobQueue.RequeueAsync(job, TimeSpan.FromSeconds(60), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JOB_EXCEPTION] JobId: {JobId}", job.JobId);
            await HandleJobFailure(job, ex.Message, cancellationToken);
        }
    }

    private async Task HandleJobFailure(JobMessage job, string errorMessage, CancellationToken cancellationToken)
    {
        if (job.RetryCount < job.MaxRetries)
        {
            // Exponential backoff: 2^retry * 5 seconds
            var delay = TimeSpan.FromSeconds(Math.Pow(2, job.RetryCount) * 5);

            _logger.LogWarning(
                "[JOB_RETRY] JobId: {JobId} | Retry: {Retry}/{MaxRetries} | Delay: {Delay}s | Error: {Error}",
                job.JobId,
                job.RetryCount + 1,
                job.MaxRetries,
                delay.TotalSeconds,
                errorMessage
            );

            await _progressTracker.MarkAsRetryingAsync(job.JobId, cancellationToken);
            await _jobQueue.RequeueAsync(job, delay, cancellationToken);
        }
        else
        {
            _logger.LogError(
                "[JOB_MAX_RETRIES] JobId: {JobId} | Moving to DLQ | Error: {Error}",
                job.JobId,
                errorMessage
            );

            await _progressTracker.MarkAsFailedAsync(job.JobId, $"Max retries exceeded: {errorMessage}", cancellationToken);
            await _jobQueue.MoveToDeadLetterQueueAsync(job, $"Max retries exceeded: {errorMessage}", cancellationToken);
            await _jobQueue.AcknowledgeAsync(job, cancellationToken);
        }
    }

    private ResiliencePipeline BuildResiliencePipeline()
    {
        var circuitBreakerThreshold = _configuration.GetValue<int>("Worker:CircuitBreakerThreshold", 5);
        var circuitBreakerDuration = _configuration.GetValue<int>("Worker:CircuitBreakerDurationSeconds", 60);

        return new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = Polly.DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "[POLLY_RETRY] Attempt: {Attempt} | Delay: {Delay}ms",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds
                    );
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = circuitBreakerThreshold,
                BreakDuration = TimeSpan.FromSeconds(circuitBreakerDuration),
                OnOpened = args =>
                {
                    _logger.LogError(
                        "[CIRCUIT_BREAKER_OPENED] BreakDuration: {Duration}s",
                        circuitBreakerDuration
                    );
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("[CIRCUIT_BREAKER_CLOSED] Circuit is healthy again");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[WORKER_STOP_REQUESTED]");
        await base.StopAsync(cancellationToken);
    }
}
