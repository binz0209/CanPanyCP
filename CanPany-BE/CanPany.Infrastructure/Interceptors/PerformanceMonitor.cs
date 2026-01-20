using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Performance monitor implementation with I18N support
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly II18nService _i18nService;
    private const long SlowOperationThresholdMs = 1000; // 1 second

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger, II18nService i18nService)
    {
        _logger = logger;
        _i18nService = i18nService;
    }

    public IDisposable StartTiming(string operationName, Dictionary<string, object?>? metadata = null)
    {
        return new PerformanceTimer(operationName, metadata, this);
    }

    public void RecordExecutionTime(string operationName, long milliseconds, Dictionary<string, object?>? metadata = null)
    {
        var logLevel = milliseconds > SlowOperationThresholdMs ? LogLevel.Warning : LogLevel.Debug;

        var logMessage = _i18nService.GetMessage(
            I18nKeys.Interceptor.Performance.Format.OperationCompleted,
            I18nContextType.Performance,
            operationName,
            milliseconds,
            metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : "N/A"
        );

        _logger.Log(logLevel, logMessage);

        // Log slow operations as warnings
        if (milliseconds > SlowOperationThresholdMs)
        {
            var slowMessage = _i18nService.GetMessage(
                I18nKeys.Interceptor.Performance.Format.SlowOperationWarning,
                I18nContextType.Performance,
                operationName,
                milliseconds,
                SlowOperationThresholdMs
            );
            
            _logger.LogWarning(slowMessage);
        }
    }

    private class PerformanceTimer : IDisposable
    {
        private readonly string _operationName;
        private readonly Dictionary<string, object?>? _metadata;
        private readonly PerformanceMonitor _monitor;
        private readonly Stopwatch _stopwatch;

        public PerformanceTimer(string operationName, Dictionary<string, object?>? metadata, PerformanceMonitor monitor)
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
