using CanPany.Application.Interfaces.Interceptors;

namespace CanPany.Worker.Infrastructure.Interceptors;

/// <summary>
/// Data masker implementation for Worker service
/// </summary>
public class WorkerDataMasker : IDataMasker
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwd", "pwd",
        "token", "access_token", "refresh_token", "bearer",
        "secret", "apikey", "api_key", "client_secret",
        "authorization", "cookie", "session",
        "creditcard", "credit_card", "cvv", "ssn",
        "encrypted", "encryptedaccesstoken"
    };

    private static readonly HashSet<string> SensitivePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "token", "secret", "key", "password"
    };

    public object? MaskSensitiveData(object? data)
    {
        if (data == null)
            return null;

        if (data is Dictionary<string, object?> dict)
            return MaskSensitiveData(dict);

        if (data is string str)
            return MaskString(str);

        return data;
    }

    public Dictionary<string, object?> MaskSensitiveData(Dictionary<string, object?>? data)
    {
        if (data == null)
            return new Dictionary<string, object?>();

        var masked = new Dictionary<string, object?>();

        foreach (var kvp in data)
        {
            if (IsSensitiveKey(kvp.Key))
            {
                masked[kvp.Key] = "***MASKED***";
            }
            else if (kvp.Value is Dictionary<string, object?> nestedDict)
            {
                masked[kvp.Key] = MaskSensitiveData(nestedDict);
            }
            else if (kvp.Value is string str && IsSensitiveValue(str))
            {
                masked[kvp.Key] = "***MASKED***";
            }
            else
            {
                masked[kvp.Key] = kvp.Value;
            }
        }

        return masked;
    }

    public bool IsSensitiveKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        if (SensitiveKeys.Contains(key))
            return true;

        return SensitivePatterns.Any(pattern => key.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static string MaskString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (value.Length > 20 && System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z0-9+/=_-]+$"))
            return "***MASKED***";

        return value;
    }

    private static bool IsSensitiveValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Length > 20 && System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z0-9+/=_-]+$"))
            return true;

        return false;
    }
}
