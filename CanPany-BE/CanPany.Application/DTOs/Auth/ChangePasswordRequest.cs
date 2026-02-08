namespace CanPany.Application.DTOs.Auth;

public record ChangePasswordRequest(string OldPassword, string NewPassword);
