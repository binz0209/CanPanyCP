using CanPany.Application.Interfaces.Services;
using System.Collections.Concurrent;

namespace CanPany.Application.Services;

/// <summary>
/// In-memory implementation of reset code storage
/// Thread-safe using ConcurrentDictionary
/// NOTE: This should be replaced with Redis in production
/// </summary>
public class InMemoryResetCodeStore : IResetCodeStore
{
    private readonly ConcurrentDictionary<string, (string Code, DateTime Expires)> _resetCodes = new();

    public void StoreCode(string email, string code, DateTime expires)
    {
        _resetCodes[email] = (code, expires);
    }

    public bool TryGetCode(string email, out string code, out DateTime expires)
    {
        if (_resetCodes.TryGetValue(email, out var data))
        {
            code = data.Code;
            expires = data.Expires;
            return true;
        }

        code = string.Empty;
        expires = DateTime.MinValue;
        return false;
    }

    public void RemoveCode(string email)
    {
        _resetCodes.TryRemove(email, out _);
    }

    public int GetCount()
    {
        return _resetCodes.Count;
    }
}
