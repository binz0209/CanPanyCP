namespace CanPany.Application.DTOs.Auth;

public record ResetPasswordRequest(string Email, string Code, string NewPassword);
