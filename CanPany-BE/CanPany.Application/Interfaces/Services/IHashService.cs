namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Password hashing service interface
/// </summary>
public interface IHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}


