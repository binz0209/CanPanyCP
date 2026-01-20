using CanPany.Application.DTOs.Auth;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Authentication controller - UC-COM-01, UC-COM-02, UC-COM-03, UC-COM-04
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly II18nService _i18nService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        II18nService i18nService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Input sanitization - trim whitespace
            request.FullName = request.FullName?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            request.Role = request.Role?.Trim() ?? "Candidate";

            // FluentValidation will automatically validate via ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.CreateError(string.Join(", ", errors), "ValidationFailed"));
            }

            // Log registration start with i18n
            var startMessage = _i18nService.GetLogMessage(I18nKeys.Logging.User.Register.Start, request.Email);
            _logger.LogInformation(startMessage);

            // Ensure role is valid (default to Candidate if empty/invalid)
            var role = string.IsNullOrWhiteSpace(request.Role) || 
                      (request.Role != "Candidate" && request.Role != "Company") 
                      ? "Candidate" 
                      : request.Role;
            
            var user = await _userService.RegisterAsync(request.FullName, request.Email, request.Password, role);
            
            // Generate token for newly registered user
            var token = await _authService.GenerateTokenAsync(user);
            
            // Log registration complete with i18n
            var completeMessage = _i18nService.GetLogMessage(I18nKeys.Logging.User.Register.Complete, user.Email, user.Id);
            _logger.LogInformation(completeMessage);
            
            // Return success message with i18n
            var successMessage = _i18nService.GetDisplayMessage(I18nKeys.Success.User.Register);
            return Ok(ApiResponse<object>.CreateSuccess(
                new { accessToken = token, user }, 
                successMessage));
        }
        catch (BusinessRuleViolationException ex)
        {
            var errorMessage = _i18nService.GetLogMessage(I18nKeys.Logging.User.Register.Start, request.Email);
            _logger.LogWarning(ex, errorMessage);
            
            var userMessage = _i18nService.GetErrorMessage(I18nKeys.Error.User.Register.EmailExists);
            return BadRequest(ApiResponse.CreateError(userMessage, "RegistrationFailed"));
        }
        catch (Exception ex)
        {
            var errorMessage = _i18nService.GetLogMessage(I18nKeys.Error.User.Register.Failed, request.Email, ex.Message);
            _logger.LogError(ex, errorMessage);
            
            var userMessage = _i18nService.GetErrorMessage(I18nKeys.Error.User.Register.Failed);
            return StatusCode(500, ApiResponse.CreateError(userMessage, "RegistrationFailed"));
        }
    }

    /// <summary>
    /// UC-COM-01: Login to System
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized(ApiResponse.CreateError("Invalid email or password", "InvalidCredentials"));
            }

            var token = await _authService.GenerateTokenAsync(user);
            return Ok(ApiResponse<object>.CreateSuccess(new { accessToken = token, user }, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse.CreateError("Login failed", "LoginFailed"));
        }
    }

    /// <summary>
    /// UC-COM-02: Logout of System
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (userId != null)
            {
                await _authService.LogoutAsync(userId, token);
            }

            return Ok(ApiResponse.CreateSuccess("Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse.CreateError("Logout failed", "LogoutFailed"));
        }
    }

    /// <summary>
    /// UC-COM-03: Change Password
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (succeeded, errors) = await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            if (!succeeded)
            {
                return BadRequest(ApiResponse.CreateError(string.Join(", ", errors), "ChangePasswordFailed"));
            }

            return Ok(ApiResponse.CreateSuccess("Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, ApiResponse.CreateError("Change password failed", "ChangePasswordFailed"));
        }
    }

    /// <summary>
    /// UC-COM-04: Reset Password (Forgot Password)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var code = await _authService.ResetPasswordAsync(request.Email);
            // In production, send code via email
            return Ok(ApiResponse<object>.CreateSuccess(new { resetCode = code }, "Reset code sent to email"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reset password code");
            return StatusCode(500, ApiResponse.CreateError("Failed to send reset code", "ResetPasswordFailed"));
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var succeeded = await _authService.VerifyResetPasswordCodeAsync(request.Email, request.Code, request.NewPassword);
            if (!succeeded)
            {
                return BadRequest(ApiResponse.CreateError("Invalid or expired reset code", "InvalidResetCode"));
            }

            return Ok(ApiResponse.CreateSuccess("Password reset successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, ApiResponse.CreateError("Reset password failed", "ResetPasswordFailed"));
        }
    }
}

public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Code, string NewPassword);


