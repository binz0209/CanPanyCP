using BCrypt.Net;
using CanPany.Application.Interfaces.Services;

namespace CanPany.Infrastructure.Security.Hashing;

/// <summary>
/// Password hashing service using BCrypt
/// </summary>
public class HashService : IHashService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

