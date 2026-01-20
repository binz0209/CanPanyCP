using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Resources;

namespace CanPany.Infrastructure.Services;

/// <summary>
/// I18N Service Implementation
/// Supports multiple context types: Logging, Display, Error, Audit, Security, Performance
/// </summary>
public class I18nService : II18nService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<I18nService> _logger;
    private readonly Dictionary<string, ResourceManager> _resourceManagers;
    private const string DefaultLanguage = "vi";

    public I18nService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<I18nService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _resourceManagers = new Dictionary<string, ResourceManager>();
        
        // Initialize resource managers for different contexts
        // Note: In production, load from actual .resx files or JSON resources
        InitializeResourceManagers();
    }

    public string GetLogMessage(string key, params object[] args)
    {
        return GetMessage(key, I18nContextType.Logging, args);
    }

    public string GetDisplayMessage(string key, params object[] args)
    {
        return GetMessage(key, I18nContextType.Display, args);
    }

    public string GetErrorMessage(string key, params object[] args)
    {
        return GetMessage(key, I18nContextType.Error, args);
    }

    public string GetMessage(string key, I18nContextType contextType, params object[] args)
    {
        try
        {
            var language = GetCurrentLanguage();
            var resourceKey = GetResourceKey(contextType, key);
            var resourceManager = GetResourceManager(contextType);
            
            // Try to get localized string
            var culture = new CultureInfo(language);
            var message = resourceManager?.GetString(resourceKey, culture);
            
            // Fallback to key if not found
            if (string.IsNullOrEmpty(message))
            {
                message = key;
                _logger.LogWarning("I18N key not found: {Key} for language {Language}", resourceKey, language);
            }
            
            // Format with arguments if provided
            if (args != null && args.Length > 0)
            {
                try
                {
                    message = string.Format(culture, message, args);
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Failed to format I18N message: {Key}", resourceKey);
                }
            }
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting I18N message for key: {Key}", key);
            return key; // Fallback to key
        }
    }

    public string GetCurrentLanguage()
    {
        // Try to get from HTTP context header
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null)
        {
            // Check Accept-Language header
            var acceptLanguage = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguage))
            {
                // Parse "en-US,en;q=0.9" -> "en"
                var language = acceptLanguage.Split(',')[0].Split(';')[0].Split('-')[0].Trim();
                if (language == "vi" || language == "en")
                    return language;
            }
            
            // Check X-Language header (custom header)
            var customLanguage = httpContext.Request.Headers["X-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(customLanguage) && (customLanguage == "vi" || customLanguage == "en"))
                return customLanguage;
        }
        
        // Fallback to default
        return DefaultLanguage;
    }

    public void SetLanguage(string language)
    {
        // Set language in HTTP context if available
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null && (language == "vi" || language == "en"))
        {
            httpContext.Items["I18nLanguage"] = language;
        }
    }

    private string GetResourceKey(I18nContextType contextType, string key)
    {
        // If key already contains "Interceptor", use as-is (it's already a full key)
        if (key.Contains("Interceptor", StringComparison.OrdinalIgnoreCase))
            return key;
        
        // Map context type to resource key prefix
        var prefix = contextType switch
        {
            I18nContextType.Logging => "Logging",
            I18nContextType.Display => "Display",
            I18nContextType.Error => "Error",
            I18nContextType.Audit => "Audit",
            I18nContextType.Security => "Security",
            I18nContextType.Performance => "Performance",
            _ => "Common"
        };
        
        // If key already has prefix, use as-is; otherwise add prefix
        if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return key;
        
        return $"{prefix}.{key}";
    }

    private ResourceManager? GetResourceManager(I18nContextType contextType)
    {
        var key = $"{contextType}";
        if (_resourceManagers.TryGetValue(key, out var manager))
            return manager;
        
        // Return default resource manager (will be initialized with in-memory resources)
        return _resourceManagers.TryGetValue("Default", out var defaultManager) ? defaultManager : null;
    }

    private void InitializeResourceManagers()
    {
        // Initialize in-memory resource managers
        // In production, load from .resx files or JSON resources
        
        // For now, create a simple in-memory resource manager
        // TODO: Replace with actual resource file loading
        
        _resourceManagers["Default"] = CreateInMemoryResourceManager();
    }

    private ResourceManager CreateInMemoryResourceManager()
    {
        var resourceSet = new Dictionary<string, Dictionary<string, string>>();
        
        // Load from JSON files
        var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "i18n");
        
        foreach (var language in new[] { "vi", "en" })
        {
            var jsonPath = Path.Combine(basePath, $"{language}.json");
            if (File.Exists(jsonPath))
            {
                try
                {
                    var jsonContent = File.ReadAllText(jsonPath);
                    var resources = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                    resourceSet[language] = FlattenJson(resources);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load I18N resources from {Path}", jsonPath);
                    resourceSet[language] = new Dictionary<string, string>();
                }
            }
            else
            {
                _logger.LogWarning("I18N resource file not found: {Path}", jsonPath);
                resourceSet[language] = new Dictionary<string, string>();
            }
        }
        
        // Return a simple resource manager wrapper
        return new InMemoryResourceManager(resourceSet);
    }
    
    private Dictionary<string, string> FlattenJson(Dictionary<string, object>? json, string prefix = "")
    {
        var result = new Dictionary<string, string>();
        
        if (json == null)
            return result;
        
        foreach (var kvp in json)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            
            if (kvp.Value is System.Text.Json.JsonElement element)
            {
                if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    // Recursively flatten nested objects
                    var nestedDict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        nestedDict[prop.Name] = prop.Value;
                    }
                    var flattened = FlattenJson(nestedDict, key);
                    foreach (var f in flattened)
                    {
                        result[f.Key] = f.Value;
                    }
                }
                else if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    result[key] = element.GetString() ?? string.Empty;
                }
            }
            else if (kvp.Value is string str)
            {
                result[key] = str;
            }
        }
        
        return result;
    }
}

/// <summary>
/// Simple in-memory resource manager for I18N
/// In production, replace with proper ResourceManager loading from .resx files
/// </summary>
internal class InMemoryResourceManager : ResourceManager
{
    private readonly Dictionary<string, Dictionary<string, string>> _resources;

    public InMemoryResourceManager(Dictionary<string, Dictionary<string, string>> resources)
        : base("InMemory", typeof(InMemoryResourceManager).Assembly)
    {
        _resources = resources;
    }

    public override string? GetString(string name, CultureInfo? culture)
    {
        var language = culture?.TwoLetterISOLanguageName ?? "vi";
        
        if (_resources.TryGetValue(language, out var langResources))
        {
            if (langResources.TryGetValue(name, out var value))
                return value;
        }
        
        // Fallback to default language
        if (language != "vi" && _resources.TryGetValue("vi", out var defaultResources))
        {
            if (defaultResources.TryGetValue(name, out var defaultValue))
                return defaultValue;
        }
        
        return null;
    }
}
