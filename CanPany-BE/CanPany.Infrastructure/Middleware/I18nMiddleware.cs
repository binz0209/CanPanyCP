using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Middleware;

/// <summary>
/// I18N Middleware - Detects language from request headers and sets it in context
/// </summary>
public class I18nMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<I18nMiddleware> _logger;

    public I18nMiddleware(RequestDelegate next, ILogger<I18nMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, II18nService i18nService)
    {
        // Detect language from headers
        var language = DetectLanguage(context);
        
        // Set language in I18N service
        i18nService.SetLanguage(language);
        
        // Store in context for easy access
        context.Items["I18nLanguage"] = language;
        
        // Add language to response headers
        context.Response.Headers["X-Language"] = language;
        
        await _next(context);
    }

    private string DetectLanguage(HttpContext context)
    {
        // Priority 1: X-Language header (custom header)
        var customLanguage = context.Request.Headers["X-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(customLanguage))
        {
            if (IsValidLanguage(customLanguage))
                return customLanguage.ToLower();
        }
        
        // Priority 2: Accept-Language header
        var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            // Parse "en-US,en;q=0.9,vi;q=0.8" -> extract languages
            var languages = acceptLanguage.Split(',')
                .Select(lang => lang.Split(';')[0].Split('-')[0].Trim().ToLower())
                .Where(IsValidLanguage)
                .ToList();
            
            if (languages.Any())
                return languages.First();
        }
        
        // Priority 3: Query string parameter
        var queryLanguage = context.Request.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(queryLanguage) && IsValidLanguage(queryLanguage))
            return queryLanguage.ToLower();
        
        // Default: Vietnamese
        return "vi";
    }

    private static bool IsValidLanguage(string language)
    {
        return language?.ToLower() == "vi" || language?.ToLower() == "en";
    }
}
