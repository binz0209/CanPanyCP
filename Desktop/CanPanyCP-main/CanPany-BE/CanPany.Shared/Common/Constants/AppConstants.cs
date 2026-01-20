namespace CanPany.Shared.Common.Constants;

public static class AppConstants
{
    public const string DefaultLanguage = "vi";
    public const string SupportedLanguages = "vi,en";
    
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    
    public const int JwtTokenExpirationMinutes = 30;
    public const int RefreshTokenExpirationDays = 7;
    
    public const int MaxFileSize = 10 * 1024 * 1024; // 10MB
    public const string AllowedFileExtensions = ".pdf,.doc,.docx,.jpg,.jpeg,.png";
    
    public const int RateLimitPerMinute = 60;
    public const int RateLimitPerHour = 1000;
}

