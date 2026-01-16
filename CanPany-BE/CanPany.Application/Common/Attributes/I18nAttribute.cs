using CanPany.Application.Interfaces.Services;

namespace CanPany.Application.Common.Attributes;

/// <summary>
/// I18N Attribute - Use this attribute to mark methods/classes that need i18n support
/// Can be used on controllers, services, or methods
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
public class I18nAttribute : Attribute
{
    /// <summary>
    /// I18N key for the message
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// Context type for the message
    /// </summary>
    public I18nContextType ContextType { get; }
    
    /// <summary>
    /// Default language if not specified in request
    /// </summary>
    public string DefaultLanguage { get; set; } = "vi";

    public I18nAttribute(string key, I18nContextType contextType = I18nContextType.Display)
    {
        Key = key;
        ContextType = contextType;
    }
}

/// <summary>
/// I18N Logging Attribute - Shortcut for logging context
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class I18nLogAttribute : I18nAttribute
{
    public I18nLogAttribute(string key) : base(key, I18nContextType.Logging)
    {
    }
}

/// <summary>
/// I18N Display Attribute - Shortcut for display context
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class I18nDisplayAttribute : I18nAttribute
{
    public I18nDisplayAttribute(string key) : base(key, I18nContextType.Display)
    {
    }
}

/// <summary>
/// I18N Error Attribute - Shortcut for error context
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class I18nErrorAttribute : I18nAttribute
{
    public I18nErrorAttribute(string key) : base(key, I18nContextType.Error)
    {
    }
}

/// <summary>
/// I18N Audit Attribute - Shortcut for audit context
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class I18nAuditAttribute : I18nAttribute
{
    public I18nAuditAttribute(string key) : base(key, I18nContextType.Audit)
    {
    }
}

/// <summary>
/// I18N Security Attribute - Shortcut for security context
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class I18nSecurityAttribute : I18nAttribute
{
    public I18nSecurityAttribute(string key) : base(key, I18nContextType.Security)
    {
    }
}
