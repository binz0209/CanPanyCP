using CanPany.Application.DTOs.Auth;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
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


