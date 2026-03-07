namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Hashing service interface - BCrypt for passwords, SHA-256 for data integrity
/// </summary>
public interface IHashService
{
    // BCrypt password hashing
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);

    // SHA-256 hashing (data integrity, checksums)
    string ComputeSHA256(string input);
    byte[] ComputeSHA256Bytes(byte[] input);

    // HMAC-SHA256 (message authentication, signing)
    string ComputeHMACSHA256(string input, string key);
    byte[] ComputeHMACSHA256Bytes(byte[] input, byte[] key);
}


