namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service for storing and retrieving password reset codes
/// </summary>
public interface IResetCodeStore
{
    /// <summary>
    /// Store a reset code for an email
    /// </summary>
    void StoreCode(string email, string code, DateTime expires);
    
    /// <summary>
    /// Try to get a reset code for an email
    /// </summary>
    bool TryGetCode(string email, out string code, out DateTime expires);
    
    /// <summary>
    /// Remove a reset code for an email
    /// </summary>
    void RemoveCode(string email);
    
    /// <summary>
    /// Get the count of stored codes (for debugging)
    /// </summary>
    int GetCount();
}
