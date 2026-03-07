using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using CanPany.Application.Interfaces.Services;

namespace CanPany.Infrastructure.Security.Hashing;

/// <summary>
/// Hashing service - BCrypt for passwords, SHA-256 for data integrity
/// </summary>
public class HashService : IHashService
{
    // ==================== BCrypt (Password Hashing) ====================

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    // ==================== SHA-256 (Data Integrity) ====================

    public string ComputeSHA256(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = ComputeSHA256Bytes(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public byte[] ComputeSHA256Bytes(byte[] input)
    {
        if (input == null || input.Length == 0)
            return Array.Empty<byte>();

        return SHA256.HashData(input);
    }

    // ==================== HMAC-SHA256 (Message Authentication) ====================

    public string ComputeHMACSHA256(string input, string key)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(key))
            return string.Empty;

        var inputBytes = Encoding.UTF8.GetBytes(input);
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var hashBytes = ComputeHMACSHA256Bytes(inputBytes, keyBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public byte[] ComputeHMACSHA256Bytes(byte[] input, byte[] key)
    {
        if (input == null || input.Length == 0 || key == null || key.Length == 0)
            return Array.Empty<byte>();

        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(input);
    }
}
