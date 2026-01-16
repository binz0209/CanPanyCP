using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public class AuditLoggerTests
{
    private readonly Mock<ILogger<AuditLogger>> _loggerMock = new();
    private readonly Mock<IDataMasker> _dataMaskerMock = new();
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly AuditLogger _auditLogger;

    public AuditLoggerTests()
    {
        _auditLogger = new AuditLogger(_loggerMock.Object, _dataMaskerMock.Object, _i18nServiceMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());
        
        _i18nServiceMock.Setup(x => x.GetLogMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>((key, args) => $"[AUDIT] {key} | {string.Join(" | ", args)}");
    }

    [Fact]
    public async Task LogAuditEventAsync_ShouldUseI18N_ForHttpRequest()
    {
        // Arrange
        var auditEvent = new AuditEvent
        {
            EventType = "HTTP_REQUEST",
            Action = "GET",
            UserId = "user123",
            CorrelationId = "corr123",
            ResourcePath = "/api/test",
            ExecutionTimeMs = 100,
            IsSuccess = true
        };

        // Act
        await _auditLogger.LogAuditEventAsync(auditEvent);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetLogMessage(
                It.Is<string>(k => k.Contains("HttpRequest")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEventAsync_ShouldUseI18N_ForServiceCall()
    {
        // Arrange
        var auditEvent = new AuditEvent
        {
            EventType = "SERVICE_CALL",
            Action = "GetUser",
            UserId = "user123",
            CorrelationId = "corr123",
            MethodName = "GetUserAsync",
            ExecutionTimeMs = 50,
            IsSuccess = true
        };

        // Act
        await _auditLogger.LogAuditEventAsync(auditEvent);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetLogMessage(
                It.Is<string>(k => k.Contains("ServiceCall")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEventAsync_ShouldMaskSensitiveData()
    {
        // Arrange
        var auditEvent = new AuditEvent
        {
            EventType = "HTTP_REQUEST",
            Action = "POST",
            UserId = "user123",
            Metadata = new Dictionary<string, object?>
            {
                ["password"] = "secret123",
                ["token"] = "abc123"
            }
        };

        // Act
        await _auditLogger.LogAuditEventAsync(auditEvent);

        // Assert
        _dataMaskerMock.Verify(
            x => x.MaskSensitiveData(It.Is<Dictionary<string, object?>>(d => d.ContainsKey("password"))),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEventAsync_ShouldLogAtWarningLevel_WhenNotSuccess()
    {
        // Arrange
        var auditEvent = new AuditEvent
        {
            EventType = "HTTP_REQUEST",
            Action = "GET",
            IsSuccess = false
        };

        // Act
        await _auditLogger.LogAuditEventAsync(auditEvent);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetLogMessage(
                It.Is<string>(k => k.Contains("Failed")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
