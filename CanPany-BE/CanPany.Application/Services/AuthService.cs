using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashService _hashService;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<string, (string Code, DateTime Expires)> _resetCodes = new(); // In-memory, should use Redis in production

    public AuthService(
        IUserRepository userRepository,
        IHashService hashService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _hashService = hashService;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
                return null;

            var isValid = _hashService.VerifyPassword(password, user.PasswordHash);
            return isValid ? user : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            return null;
        }
    }

    public Task<string> GenerateTokenAsync(User user)
    {
        // TODO: Implement JWT token generation
        // This should be implemented with JWT service
        throw new NotImplementedException("JWT token generation not implemented yet");
    }

    public Task<bool> LogoutAsync(string userId, string token)
    {
        // TODO: Implement token blacklist or revocation
        // This should invalidate the token
        return Task.FromResult(true);
    }

    public async Task<string> ResetPasswordAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Generate reset code (6 digits)
            var code = new Random().Next(100000, 999999).ToString();
            var expires = DateTime.UtcNow.AddMinutes(15);

            _resetCodes[email] = (code, expires);

            _logger.LogInformation("Password reset code generated for {Email}", email);
            return code; // In production, send via email
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating reset password code: {Email}", email);
            throw;
        }
    }

    public async Task<bool> VerifyResetPasswordCodeAsync(string email, string code, string newPassword)
    {
        try
        {
            if (!_resetCodes.TryGetValue(email, out var resetData))
                return false;

            if (resetData.Expires < DateTime.UtcNow)
            {
                _resetCodes.Remove(email);
                return false;
            }

            if (resetData.Code != code)
                return false;

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            user.PasswordHash = _hashService.HashPassword(newPassword);
            user.MarkAsUpdated();
            await _userRepository.UpdateAsync(user);

            _resetCodes.Remove(email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reset password code: {Email}", email);
            return false;
        }
    }
}

