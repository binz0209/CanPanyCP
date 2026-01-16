using CanPany.Application.Interfaces.Interceptors;
using CanPany.Infrastructure.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public interface ITestService
{
    string GetValue(string input);
    Task<string> GetValueAsync(string input);
    void ThrowException();
}

public class TestService : ITestService
{
    public string GetValue(string input) => $"Processed: {input}";
    
    public async Task<string> GetValueAsync(string input)
    {
        await Task.Delay(10);
        return $"Processed: {input}";
    }
    
    public void ThrowException() => throw new InvalidOperationException("Test exception");
}

public class ServiceInterceptorTests
{
    private readonly Mock<IAuditLogger> _auditLoggerMock = new();
    private readonly Mock<IPerformanceMonitor> _performanceMonitorMock = new();
    private readonly Mock<IExceptionCapture> _exceptionCaptureMock = new();
    private readonly Mock<IDataMasker> _dataMaskerMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILogger<ServiceInterceptor<ITestService>>> _loggerMock = new();

    [Fact]
    public void CreateInterceptor_ShouldWrapServiceCall()
    {
        // Arrange
        var target = new TestService();
        var interceptor = ServiceInterceptor<ITestService>.Create(
            target,
            _auditLoggerMock.Object,
            _performanceMonitorMock.Object,
            _exceptionCaptureMock.Object,
            _dataMaskerMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<object>()))
            .Returns<object>(o => o);

        // Act
        var result = interceptor.GetValue("test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Processed: test", result);
        _auditLoggerMock.Verify(
            x => x.LogAuditEventAsync(It.Is<AuditEvent>(e =>
                e.EventType == "SERVICE_CALL" &&
                e.MethodName.Contains("GetValue"))),
            Times.Once);
    }

    [Fact]
    public async Task CreateInterceptor_ShouldWrapAsyncServiceCall()
    {
        // Arrange
        var target = new TestService();
        var interceptor = ServiceInterceptor<ITestService>.Create(
            target,
            _auditLoggerMock.Object,
            _performanceMonitorMock.Object,
            _exceptionCaptureMock.Object,
            _dataMaskerMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<object>()))
            .Returns<object>(o => o);

        // Act
        var result = await interceptor.GetValueAsync("test");

        // Assert
        Assert.Equal("Processed: test", result);
    }

    [Fact]
    public void CreateInterceptor_ShouldCaptureException()
    {
        // Arrange
        var target = new TestService();
        var interceptor = ServiceInterceptor<ITestService>.Create(
            target,
            _auditLoggerMock.Object,
            _performanceMonitorMock.Object,
            _exceptionCaptureMock.Object,
            _dataMaskerMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<object>()))
            .Returns<object>(o => o);

        // Act & Assert
        // Note: Reflection wraps exceptions in TargetInvocationException, but we unwrap it
        var exception = Assert.ThrowsAny<Exception>(() => interceptor.ThrowException());
        Assert.True(exception is InvalidOperationException || 
                   (exception is System.Reflection.TargetInvocationException tie && tie.InnerException is InvalidOperationException));

        _exceptionCaptureMock.Verify(
            x => x.CaptureExceptionAsync(
                It.IsAny<InvalidOperationException>(),
                It.IsAny<ExceptionContext>()),
            Times.Once);
    }
}
