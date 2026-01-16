namespace CanPany.Application.Interfaces.Interceptors;

/// <summary>
/// Data masking interface for sensitive information
/// </summary>
public interface IDataMasker
{
    /// <summary>
    /// Mask sensitive data in object
    /// </summary>
    object? MaskSensitiveData(object? data);
    
    /// <summary>
    /// Mask sensitive data in dictionary
    /// </summary>
    Dictionary<string, object?> MaskSensitiveData(Dictionary<string, object?>? data);
    
    /// <summary>
    /// Check if key contains sensitive information
    /// </summary>
    bool IsSensitiveKey(string key);
}
