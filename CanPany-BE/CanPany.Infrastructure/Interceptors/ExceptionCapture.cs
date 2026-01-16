using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Interceptors;

/// <summary>
/// Exception capture implementation with I18N support
/// </summary>
public class ExceptionCapture : IExceptionCapture
{
    private readonly ILogger<ExceptionCapture> _logger;
    private readonly IDataMasker _dataMasker;
    private readonly II18nService _i18nService;

    public ExceptionCapture(
        ILogger<ExceptionCapture> logger, 
        IDataMasker dataMasker,
        II18nService i18nService)
    {
        _logger = logger;
        _dataMasker = dataMasker;
        _i18nService = i18nService;
    }

    public Task CaptureExceptionAsync(Exception exception, ExceptionContext context)
    {
        try
        {
            // Mask sensitive data in metadata
            var maskedMetadata = context.Metadata != null
                ? _dataMasker.MaskSensitiveData(context.Metadata)
                : null;

            var logMessage = _i18nService.GetMessage(
                I18nKeys.Interceptor.Exception.Format.ExceptionOccurred,
                I18nContextType.Logging,
                exception.GetType().Name,
                context.UserId ?? "Anonymous",
                context.CorrelationId ?? "N/A",
                context.RequestPath ?? context.MethodName ?? "N/A",
                context.HttpMethod ?? "N/A",
                context.ServiceName ?? "N/A",
                context.MethodName ?? "N/A"
            );

            _logger.LogError(exception, logMessage);

            // Log metadata if present (masked)
            if (maskedMetadata != null && maskedMetadata.Count > 0)
            {
                _logger.LogDebug(
                    "[EXCEPTION_METADATA] {ExceptionType} | {Metadata}",
                    exception.GetType().Name,
                    System.Text.Json.JsonSerializer.Serialize(maskedMetadata)
                );
            }

            // Log stack trace for critical exceptions
            if (exception is System.Security.SecurityException ||
                exception is UnauthorizedAccessException)
            {
                var criticalMessage = _i18nService.GetMessage(
                    I18nKeys.Interceptor.Exception.Format.CriticalException,
                    I18nContextType.Logging,
                    exception.GetType().Name,
                    exception.StackTrace ?? "N/A"
                );
                
                _logger.LogCritical(exception, criticalMessage);
            }
        }
        catch (Exception ex)
        {
            // Don't let exception capture break the application
            _logger.LogError(ex, "Failed to capture exception");
        }

        return Task.CompletedTask;
    }
}
