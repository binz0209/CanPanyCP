using CanPany.Application.Interfaces.Interceptors;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CanPany.Worker.Infrastructure.Interceptors;

/// <summary>
/// Simplified performance monitor for Worker service (no I18N dependency)
/// </summary>
public class WorkerPerformanceMonitor : IPerformanceMonitor
{
    private readonly ILogger<WorkerPerformanceMonitor> _logger;
    private const long SlowOperationThresholdMs = 1000;

    public WorkerPerformanceMonitor(ILogger<WorkerPerformanceMonitor> logger)
    {
        _logger = logger;
    }

    public IDisposable StartTiming(string operationName, Dictionary<string, object?>? metadata = null)
    {
        return new PerformanceTimer(operationName, metadata, this);
    }

    public void RecordExecutionTime(string operationName, long milliseconds, Dictionary<string, object?>? metadata = null)
    {
        var logLevel = milliseconds > SlowOperationThresholdMs ? LogLevel.Warning : LogLevel.Debug;

        _logger.Log(
            logLevel,
            "[PERF] Operation: {Operation} | Duration: {Duration}ms | Metadata: {Metadata}",
            operationName,
            milliseconds,
            metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : "N/A"
        );

        if (milliseconds > SlowOperationThresholdMs)
        {
            _logger.LogWarning(
                "[PERF_SLOW] Operation: {Operation} | Duration: {Duration}ms | Threshold: {Threshold}ms",
                operationName,
                milliseconds,
                SlowOperationThresholdMs
            );
        }
    }

    private class PerformanceTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly Dictionary<string, object?>? _metadata;
        private readonly WorkerPerformanceMonitor _monitor;
        private readonly Stopwatch _stopwatch;

        public PerformanceTimer(string operationName, Dictionary<string, object?>? metadata, WorkerPerformanceMonitor monitor)
        {
            _operationName = operationName;
            _metadata = metadata;
            _monitor = monitor;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _monitor.RecordExecutionTime(_operationName, _stopwatch.ElapsedMilliseconds, _metadata);
        }
    }
}
