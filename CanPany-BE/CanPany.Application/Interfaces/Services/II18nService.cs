namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// I18N Service - Internationalization service for multiple contexts
/// </summary>
public interface II18nService
{
    /// <summary>
    /// Get localized message for logging context
    /// </summary>
    string GetLogMessage(string key, params object[] args);
    
    /// <summary>
    /// Get localized message for display context (API responses, UI)
    /// </summary>
    string GetDisplayMessage(string key, params object[] args);
    
    /// <summary>
    /// Get localized error message
    /// </summary>
    string GetErrorMessage(string key, params object[] args);
    
    /// <summary>
    /// Get localized message with custom context type
    /// </summary>
    string GetMessage(string key, I18nContextType contextType, params object[] args);
    
    /// <summary>
    /// Get current language/culture
    /// </summary>
    string GetCurrentLanguage();
    
    /// <summary>
    /// Set language/culture
    /// </summary>
    void SetLanguage(string language);
}

/// <summary>
/// I18N Context Types - Different contexts for different use cases
/// </summary>
public enum I18nContextType
{
    /// <summary>
    /// For logging - technical messages, may include more details
    /// </summary>
    Logging,
    
    /// <summary>
    /// For display - user-facing messages, friendly and concise
    /// </summary>
    Display,
    
    /// <summary>
    /// For errors - error messages shown to users
    /// </summary>
    Error,
    
    /// <summary>
    /// For audit - audit log messages
    /// </summary>
    Audit,
    
    /// <summary>
    /// For security - security event messages
    /// </summary>
    Security,
    
    /// <summary>
    /// For performance - performance monitoring messages
    /// </summary>
    Performance
}
