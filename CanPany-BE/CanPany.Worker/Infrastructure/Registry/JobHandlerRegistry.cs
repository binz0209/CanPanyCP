using CanPany.Worker.Handlers;
using Microsoft.Extensions.Logging;

namespace CanPany.Worker.Infrastructure.Registry;

/// <summary>
/// Registry for job handlers - maps I18N keys to handlers
/// </summary>
public class JobHandlerRegistry
{
    private readonly Dictionary<string, IJobHandler> _exactMatches = new();
    private readonly List<IJobHandler> _patternHandlers = new();
    private readonly ILogger<JobHandlerRegistry> _logger;

    public JobHandlerRegistry(ILogger<JobHandlerRegistry> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a job handler
    /// </summary>
    public void Register(IJobHandler handler)
    {
        foreach (var key in handler.SupportedI18nKeys)
        {
            if (key.Contains('*'))
            {
                // Pattern-based handler
                _patternHandlers.Add(handler);
            }
            else
            {
                // Exact match handler
                _exactMatches[key] = handler;
            }
        }

        _logger.LogInformation(
            "[HANDLER_REGISTERED] {HandlerType} | Keys: {Keys}",
            handler.GetType().Name,
            string.Join(", ", handler.SupportedI18nKeys)
        );
    }

    /// <summary>
    /// Get handler for I18N key
    /// </summary>
    public IJobHandler? GetHandler(string i18nKey)
    {
        // Try exact match first (faster)
        if (_exactMatches.TryGetValue(i18nKey, out var handler))
            return handler;

        // Try pattern matching
        foreach (var patternHandler in _patternHandlers)
        {
            if (patternHandler.CanHandle(i18nKey))
                return patternHandler;
        }

        _logger.LogWarning("[HANDLER_NOT_FOUND] I18nKey: {I18nKey}", i18nKey);
        return null;
    }

    /// <summary>
    /// Get all registered handlers
    /// </summary>
    public IEnumerable<IJobHandler> GetAllHandlers()
    {
        return _exactMatches.Values.Concat(_patternHandlers).Distinct();
    }
}
