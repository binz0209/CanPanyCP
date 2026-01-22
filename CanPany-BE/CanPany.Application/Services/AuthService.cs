using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace CanPany.Application.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashService _hashService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IBackgroundEmailService _backgroundEmailService;
    private readonly Dictionary<string, (string Code, DateTime Expires)> _resetCodes = new(); // In-memory, should use Redis in production

    public AuthService(
        IUserRepository userRepository,
        IHashService hashService,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IBackgroundEmailService backgroundEmailService)
    {
        _userRepository = userRepository;
        _hashService = hashService;
        _logger = logger;
        _configuration = configuration;
        _backgroundEmailService = backgroundEmailService;
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
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "CanPany";
            var audience = jwtSettings["Audience"] ?? "CanPanyUsers";
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "30");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("sub", user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(tokenString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {UserId}", user.Id);
            throw;
        }
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

            // Queue email to be sent asynchronously
            _backgroundEmailService.QueuePasswordResetEmail(email, user.FullName, code);

            _logger.LogInformation("Password reset code generated and email queued for {Email}", email);
            return code; // In production, don't return code - only send via email
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

