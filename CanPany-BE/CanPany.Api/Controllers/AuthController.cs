using CanPany.Application.DTOs.Auth;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Exceptions;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using CanPany.Application.Common.Attributes;
using CanPany.Domain.Entities;

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
    private readonly IMemoryCache _cache;
    private readonly IUserProfileService _profileService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        II18nService i18nService,
        IMemoryCache cache,
        IUserProfileService profileService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _i18nService = i18nService;
        _cache = cache;
        _profileService = profileService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [AuditLog("User", "auditLogs.actions.register")]
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
    [AuditLog("User", "auditLogs.actions.login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.AuthenticateAsync(request.Email, request.Password);
            if (user == null)
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.User.Login.InvalidCredentials);
                return Unauthorized(ApiResponse.CreateError(errorMsg, "InvalidCredentials"));
            }

            var token = await _authService.GenerateTokenAsync(user);
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.User.Login);
            return Ok(ApiResponse<object>.CreateSuccess(new { accessToken = token, user }, successMsg));
        }
        catch (Exception ex)
        {
            var logMsg = _i18nService.GetLogMessage(I18nKeys.Error.User.Login.Failed, request.Email, ex.Message);
            _logger.LogError(ex, logMsg);
            
            var userMsg = _i18nService.GetErrorMessage(I18nKeys.Error.User.Login.Failed);
            return StatusCode(500, ApiResponse.CreateError(userMsg, "LoginFailed"));
        }
    }

    /// <summary>
    /// UC-COM-02: Logout of System
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [AuditLog("User", "auditLogs.actions.logout")]
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

            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.User.Logout);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
    [AuditLog("User", "auditLogs.actions.changePassword")]
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

            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.User.PasswordChange);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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

    /// <summary>
    /// Step 1: Generate GitHub OAuth URL — FE dùng URL này để redirect user sang GitHub
    /// </summary>
    [Authorize]
    [HttpGet("github/link")]
    public IActionResult GetGitHubLinkUrl()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var clientId = _configuration["OAuth:GitHub:ClientId"];
            if (string.IsNullOrEmpty(clientId))
                return StatusCode(500, ApiResponse.CreateError("GitHub OAuth chưa được cấu hình", "NotConfigured"));

            // Tạo state token ngẫu nhiên để chống CSRF
            var state = Guid.NewGuid().ToString("N");
            _cache.Set($"gh_oauth_{state}", userId, TimeSpan.FromMinutes(10));

            var callbackUrl = Uri.EscapeDataString(_configuration["OAuth:GitHub:RedirectUri"] ?? "");
            var scope = Uri.EscapeDataString("read:user user:email");
            var oauthUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={callbackUrl}&state={state}&scope={scope}";

            var logMsg = _i18nService.GetLogMessage(I18nKeys.Interceptor.Audit.GitHubLink, userId);
            _logger.LogInformation(logMsg);

            return Ok(ApiResponse<object>.CreateSuccess(new { oauthUrl }, "GitHub OAuth URL generated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_LINK] Error generating OAuth URL");
            return StatusCode(500, ApiResponse.CreateError("Không thể tạo OAuth URL", "OAuthFailed"));
        }
    }

    /// <summary>
    /// Step 2: GitHub callback — GitHub redirect browser về đây sau khi user authorize
    /// </summary>
    [AllowAnonymous]
    [HttpGet("github/callback")]
    public async Task<IActionResult> GitHubCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        var frontendUrl = _configuration["OAuth:GitHub:FrontendCallbackUrl"] ?? "http://localhost:5173/profile";

        // User từ chối cấp quyền
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("[GITHUB_CALLBACK] User denied access: {Error}", error);
            return Redirect($"{frontendUrl}?github_linked=false&error={Uri.EscapeDataString(error)}");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return Redirect($"{frontendUrl}?github_linked=false&error=missing_params");

        // Xác minh CSRF state
        if (!_cache.TryGetValue($"gh_oauth_{state}", out string? userId) || string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("[GITHUB_CALLBACK] Invalid or expired state token");
            return Redirect($"{frontendUrl}?github_linked=false&error=invalid_state");
        }
        _cache.Remove($"gh_oauth_{state}");

        try
        {
            var clientId = _configuration["OAuth:GitHub:ClientId"];
            var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

            // Đổi code lấy access token
            var tokenHttp = _httpClientFactory.CreateClient();
            tokenHttp.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            tokenHttp.DefaultRequestHeaders.UserAgent.ParseAdd("CanPany/1.0");

            var tokenResp = await tokenHttp.PostAsJsonAsync(
                "https://github.com/login/oauth/access_token",
                new { client_id = clientId, client_secret = clientSecret, code });

            if (!tokenResp.IsSuccessStatusCode)
            {
                _logger.LogError("[GITHUB_CALLBACK] Token exchange failed: {Status}", tokenResp.StatusCode);
                return Redirect($"{frontendUrl}?github_linked=false&error=token_exchange_failed");
            }

            var tokenData = await tokenResp.Content.ReadFromJsonAsync<GitHubTokenResponse>();
            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                _logger.LogError("[GITHUB_CALLBACK] Empty access token for user {UserId}", userId);
                return Redirect($"{frontendUrl}?github_linked=false&error=empty_token");
            }

            // Lấy thông tin GitHub user
            var userHttp = _httpClientFactory.CreateClient();
            userHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
            userHttp.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            userHttp.DefaultRequestHeaders.UserAgent.ParseAdd("CanPany/1.0");

            var userResp = await userHttp.GetAsync("https://api.github.com/user");
            if (!userResp.IsSuccessStatusCode)
            {
                _logger.LogError("[GITHUB_CALLBACK] Failed to fetch GitHub user: {Status}", userResp.StatusCode);
                return Redirect($"{frontendUrl}?github_linked=false&error=user_fetch_failed");
            }

            var gitHubUser = await userResp.Content.ReadFromJsonAsync<GitHubUserResponse>();
            if (gitHubUser == null || string.IsNullOrEmpty(gitHubUser.Login))
                return Redirect($"{frontendUrl}?github_linked=false&error=user_parse_failed");

            // Lưu vào UserProfile
            var gitHubUrl = gitHubUser.HtmlUrl ?? $"https://github.com/{gitHubUser.Login}";
            var linked = await _profileService.LinkGitHubAsync(userId, gitHubUser.Login, gitHubUrl);

            if (!linked)
                return Redirect($"{frontendUrl}?github_linked=false&error=profile_update_failed");

            var logMsg = _i18nService.GetLogMessage(I18nKeys.Interceptor.Audit.GitHubCallback, gitHubUser.Login, userId);
            _logger.LogInformation(logMsg);

            // Generate a fresh token so the frontend gets the updated claims (if any)
            var user = await _userService.GetByIdAsync(userId);
            string? newTokenParam = "";
            if (user != null)
            {
                var newToken = await _authService.GenerateTokenAsync(user);
                newTokenParam = $"&token={newToken}";
            }

            return Redirect($"{frontendUrl}?github_linked=true&github_username={Uri.EscapeDataString(gitHubUser.Login)}{newTokenParam}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GITHUB_CALLBACK] Unexpected error for user {UserId}", userId);
            return Redirect($"{frontendUrl}?github_linked=false&error=server_error");
        }
    }

    /// <summary>
    /// Step 1: Generate Google OAuth URL for SSO 
    /// </summary>
    [AllowAnonymous]
    [HttpGet("google/link")]
    public IActionResult GetGoogleLinkUrl([FromQuery] string? role = null)
    {
        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            if (string.IsNullOrEmpty(clientId))
                return StatusCode(500, ApiResponse.CreateError("Google OAuth chưa được cấu hình", "NotConfigured"));

            // Generate state token
            var state = Guid.NewGuid().ToString("N");
            // Store role in cache if user is trying to register with a specific role via Google SSO
            _cache.Set($"google_oauth_{state}", role ?? "Candidate", TimeSpan.FromMinutes(10));

            var callbackUrl = Uri.EscapeDataString(_configuration["OAuth:Google:RedirectUri"] ?? "");
            var scope = Uri.EscapeDataString("openid email profile");
            var oauthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={callbackUrl}&response_type=code&scope={scope}&state={state}&access_type=offline";

            _logger.LogInformation("[GOOGLE_LINK] Generated OAuth URL for role {Role}", role ?? "Candidate");

            return Ok(ApiResponse<object>.CreateSuccess(new { oauthUrl }, "Google OAuth URL generated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GOOGLE_LINK] Error generating OAuth URL");
            return StatusCode(500, ApiResponse.CreateError("Không thể tạo OAuth URL", "OAuthFailed"));
        }
    }

    /// <summary>
    /// Step 2: Google callback for SSO
    /// </summary>
    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error)
    {
        var frontendUrl = _configuration["OAuth:Google:FrontendCallbackUrl"] ?? "http://localhost:5173/auth/callback";

        // User denied access
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("[GOOGLE_CALLBACK] User denied access: {Error}", error);
            return Redirect($"{frontendUrl}?google_sso=false&error={Uri.EscapeDataString(error)}");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return Redirect($"{frontendUrl}?google_sso=false&error=missing_params");

        // Verify state token
        if (!_cache.TryGetValue($"google_oauth_{state}", out string? requestedRole))
        {
            _logger.LogWarning("[GOOGLE_CALLBACK] Invalid or expired state token");
            return Redirect($"{frontendUrl}?google_sso=false&error=invalid_state");
        }
        _cache.Remove($"google_oauth_{state}");

        try
        {
            var clientId = _configuration["OAuth:Google:ClientId"];
            var clientSecret = _configuration["OAuth:Google:ClientSecret"];
            var redirectUri = _configuration["OAuth:Google:RedirectUri"];

            // Exchange code for access token
            var tokenHttp = _httpClientFactory.CreateClient();
            var tokenResp = await tokenHttp.PostAsJsonAsync("https://oauth2.googleapis.com/token", new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code,
                grant_type = "authorization_code",
                redirect_uri = redirectUri
            });

            if (!tokenResp.IsSuccessStatusCode)
            {
                _logger.LogError("[GOOGLE_CALLBACK] Token exchange failed: {Status}", tokenResp.StatusCode);
                return Redirect($"{frontendUrl}?google_sso=false&error=token_exchange_failed");
            }

            var tokenData = await tokenResp.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                _logger.LogError("[GOOGLE_CALLBACK] Empty access token");
                return Redirect($"{frontendUrl}?google_sso=false&error=empty_token");
            }

            // Get Google User info
            var userHttp = _httpClientFactory.CreateClient();
            userHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var userResp = await userHttp.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            if (!userResp.IsSuccessStatusCode)
            {
                _logger.LogError("[GOOGLE_CALLBACK] Failed to fetch Google user: {Status}", userResp.StatusCode);
                return Redirect($"{frontendUrl}?google_sso=false&error=user_fetch_failed");
            }

            var googleUser = await userResp.Content.ReadFromJsonAsync<GoogleUserResponse>();
            if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
            {
                _logger.LogError("[GOOGLE_CALLBACK] Failed to parse Google user or Email empty");
                return Redirect($"{frontendUrl}?google_sso=false&error=user_parse_failed");
            }

            // SSO Login / Registration Logic
            var existingUser = await _userService.GetByEmailAsync(googleUser.Email);
            User user;

            if (existingUser != null)
            {
                user = existingUser;
            }
            else
            {
                // Register new user with random password
                var randomPassword = Guid.NewGuid().ToString("N") + "Aa1@";
                user = await _userService.RegisterAsync(
                    googleUser.Name ?? googleUser.Email, 
                    googleUser.Email, 
                    randomPassword, 
                    requestedRole ?? "Candidate");

                // Update avatar if provided
                if (!string.IsNullOrEmpty(googleUser.Picture))
                {
                    user.AvatarUrl = googleUser.Picture;
                    await _userService.UpdateAsync(user.Id, user);
                }
            }

            // Generate JWT Token
            var token = await _authService.GenerateTokenAsync(user);

            _logger.LogInformation("[GOOGLE_CALLBACK] SSO successful for user {Email}", user.Email);

            return Redirect($"{frontendUrl}?google_sso=true&token={token}&email={Uri.EscapeDataString(user.Email)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GOOGLE_CALLBACK] Unexpected error during Google SSO");
            return Redirect($"{frontendUrl}?google_sso=false&error=server_error");
        }
    }
}

// DTO nội bộ cho Google OAuth token response
file record GoogleTokenResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string? AccessToken,
    [property: System.Text.Json.Serialization.JsonPropertyName("token_type")] string? TokenType,
    [property: System.Text.Json.Serialization.JsonPropertyName("expires_in")] int? ExpiresIn);

// DTO nội bộ cho Google user info
file record GoogleUserResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")] string? Id,
    [property: System.Text.Json.Serialization.JsonPropertyName("email")] string? Email,
    [property: System.Text.Json.Serialization.JsonPropertyName("name")] string? Name,
    [property: System.Text.Json.Serialization.JsonPropertyName("picture")] string? Picture);

// DTO nội bộ cho GitHub OAuth token response
file record GitHubTokenResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("access_token")] string? AccessToken,
    [property: System.Text.Json.Serialization.JsonPropertyName("token_type")] string? TokenType,
    [property: System.Text.Json.Serialization.JsonPropertyName("scope")] string? Scope);

// DTO nội bộ cho GitHub user info
file record GitHubUserResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("login")] string? Login,
    [property: System.Text.Json.Serialization.JsonPropertyName("html_url")] string? HtmlUrl,
    [property: System.Text.Json.Serialization.JsonPropertyName("name")] string? Name,
    [property: System.Text.Json.Serialization.JsonPropertyName("avatar_url")] string? AvatarUrl);
