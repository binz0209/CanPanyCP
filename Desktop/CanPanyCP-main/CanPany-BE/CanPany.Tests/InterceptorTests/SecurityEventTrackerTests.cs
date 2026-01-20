using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public class SecurityEventTrackerTests
{
    private readonly Mock<ILogger<SecurityEventTracker>> _loggerMock = new();
    private readonly Mock<IDataMasker> _dataMaskerMock = new();
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly SecurityEventTracker _securityTracker;

    public SecurityEventTrackerTests()
    {
        _securityTracker = new SecurityEventTracker(_loggerMock.Object, _dataMaskerMock.Object, _i18nServiceMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());
        
        _i18nServiceMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<I18nContextType>(), It.IsAny<object[]>()))
            .Returns<string, I18nContextType, object[]>((key, context, args) => $"[SECURITY] {key} | {string.Join(" | ", args)}");
    }

    [Fact]
    public async Task TrackSecurityEventAsync_ShouldUseI18N_ForAuthentication()
    {
        // Arrange
        var securityEvent = new SecurityEvent
        {
            EventType = "AUTHENTICATION",
            Action = "Login",
            UserId = "user123",
            ResourcePath = "/api/auth/login",
            IpAddress = "127.0.0.1",
            IsSuccess = true,
            Severity = "INFO"
        };

        // Act
        await _securityTracker.TrackSecurityEventAsync(securityEvent);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("Authentication")),
                I18nContextType.Security,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task TrackSecurityEventAsync_ShouldUseI18N_ForAuthorization()
    {
        // Arrange
        var securityEvent = new SecurityEvent
        {
            EventType = "AUTHORIZATION",
            Action = "AccessDenied",
            UserId = "user123",
            ResourcePath = "/api/admin/users",
            IpAddress = "127.0.0.1",
            IsSuccess = false,
            Severity = "WARNING"
        };

        // Act
        await _securityTracker.TrackSecurityEventAsync(securityEvent);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("Authorization")),
                I18nContextType.Security,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task TrackSecurityEventAsync_ShouldMaskSensitiveData()
    {
        // Arrange
        var securityEvent = new SecurityEvent
        {
            EventType = "AUTHENTICATION",
            Action = "Login",
            Metadata = new Dictionary<string, object?>
            {
                ["password"] = "secret123",
                ["token"] = "abc123"
            }
        };

        // Act
        await _securityTracker.TrackSecurityEventAsync(securityEvent);

        // Assert
        _dataMaskerMock.Verify(
            x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()),
            Times.Once);
    }
}
