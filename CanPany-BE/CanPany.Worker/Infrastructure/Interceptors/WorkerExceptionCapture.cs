using CanPany.Application.Interfaces.Interceptors;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Infrastructure.Interceptors;

/// <summary>
/// Simplified exception capture for Worker service (no I18N dependency)
/// </summary>
public class WorkerExceptionCapture : IExceptionCapture
{
    private readonly ILogger<WorkerExceptionCapture> _logger;
    private readonly IDataMasker _dataMasker;

    public WorkerExceptionCapture(
        ILogger<WorkerExceptionCapture> logger,
        IDataMasker dataMasker)
    {
        _logger = logger;
        _dataMasker = dataMasker;
    }

    public Task CaptureExceptionAsync(Exception exception, ExceptionContext context)
    {
        try
        {
            var maskedMetadata = context.Metadata != null
                ? _dataMasker.MaskSensitiveData(context.Metadata)
                : null;

            _logger.LogError(
                exception,
                "[EXCEPTION] Type: {ExceptionType} | Service: {Service} | Method: {Method} | Time: {Timestamp}",
                exception.GetType().Name,
                context.ServiceName ?? "N/A",
                context.MethodName ?? "N/A",
                context.Timestamp
            );

            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[EXCEPTION_METADATA] {ExceptionType} | {Metadata}",
                    exception.GetType().Name,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata)
                );
            }

            if (exception is System.Security.SecurityException ||
                exception is UnauthorizedAccessException)
            {
                _logger.LogCritical(
                    exception,
                    "[CRITICAL_EXCEPTION] Type: {ExceptionType} | StackTrace: {StackTrace}",
                    exception.GetType().Name,
                    exception.StackTrace ?? "N/A"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture exception");
        }

        return Task.CompletedTask;
    }
}
