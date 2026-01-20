using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<string> GenerateTokenAsync(User user);
    Task<bool> LogoutAsync(string userId, string token);
    Task<string> ResetPasswordAsync(string email);
    Task<bool> VerifyResetPasswordCodeAsync(string email, string code, string newPassword);
}


