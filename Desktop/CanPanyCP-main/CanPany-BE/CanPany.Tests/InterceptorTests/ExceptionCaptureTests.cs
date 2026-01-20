using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public class ExceptionCaptureTests
{
    private readonly Mock<ILogger<ExceptionCapture>> _loggerMock = new();
    private readonly Mock<IDataMasker> _dataMaskerMock = new();
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly ExceptionCapture _exceptionCapture;

    public ExceptionCaptureTests()
    {
        _exceptionCapture = new ExceptionCapture(_loggerMock.Object, _dataMaskerMock.Object, _i18nServiceMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());
        
        _i18nServiceMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<I18nContextType>(), It.IsAny<object[]>()))
            .Returns<string, I18nContextType, object[]>((key, context, args) => $"[EXCEPTION] {key} | {string.Join(" | ", args)}");
    }

    [Fact]
    public async Task CaptureExceptionAsync_ShouldUseI18N()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var context = new ExceptionContext
        {
            UserId = "user123",
            CorrelationId = "corr123",
            RequestPath = "/api/test",
            HttpMethod = "GET",
            ServiceName = "TestService",
            MethodName = "TestMethod"
        };

        // Act
        await _exceptionCapture.CaptureExceptionAsync(exception, context);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("ExceptionOccurred")),
                I18nContextType.Logging,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureExceptionAsync_ShouldUseI18N_ForCriticalException()
    {
        // Arrange
        var exception = new System.Security.SecurityException("Security violation");
        var context = new ExceptionContext
        {
            UserId = "user123",
            RequestPath = "/api/test"
        };

        // Act
        await _exceptionCapture.CaptureExceptionAsync(exception, context);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("CriticalException")),
                I18nContextType.Logging,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureExceptionAsync_ShouldMaskSensitiveData()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var context = new ExceptionContext
        {
            Metadata = new Dictionary<string, object?>
            {
                ["password"] = "secret123",
                ["token"] = "abc123"
            }
        };

        // Act
        await _exceptionCapture.CaptureExceptionAsync(exception, context);

        // Assert
        _dataMaskerMock.Verify(
            x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()),
            Times.Once);
    }
}
