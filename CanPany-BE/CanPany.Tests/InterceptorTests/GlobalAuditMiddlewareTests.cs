using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace CanPany.Tests.InterceptorTests;

public class GlobalAuditMiddlewareTests
{
    private readonly Mock<IAuditLogger> _auditLoggerMock = new();
    private readonly Mock<IPerformanceMonitor> _performanceMonitorMock = new();
    private readonly Mock<ISecurityEventTracker> _securityTrackerMock = new();
    private readonly Mock<IExceptionCapture> _exceptionCaptureMock = new();
    private readonly Mock<IDataMasker> _dataMaskerMock = new();
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly Mock<ILogger<GlobalAuditMiddleware>> _loggerMock = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
    private readonly Mock<IServiceScope> _serviceScopeMock = new();
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GlobalAuditMiddlewareTests()
    {
        // Create a real service collection and register mocks
        var services = new ServiceCollection();
        services.AddSingleton(_auditLoggerMock.Object);
        services.AddSingleton(_performanceMonitorMock.Object);
        services.AddSingleton(_securityTrackerMock.Object);
        services.AddSingleton(_exceptionCaptureMock.Object);
        
        _serviceProvider = services.BuildServiceProvider();
        _serviceScopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogAuditEvent_ForSuccessfulRequest()
    {
        // Arrange
        var context = CreateHttpContext("/api/test", "GET");
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new GlobalAuditMiddleware(
            next,
            _serviceScopeFactory,
            _dataMaskerMock.Object,
            _loggerMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _auditLoggerMock.Verify(
            x => x.LogAuditEventAsync(It.Is<AuditEvent>(e =>
                e.EventType == "HTTP_REQUEST" &&
                e.Action == "GET" &&
                e.ResourcePath == "/api/test" &&
                e.IsSuccess == true)),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSkipAudit_ForHealthCheck()
    {
        // Arrange
        var context = CreateHttpContext("/health", "GET");
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new GlobalAuditMiddleware(
            next,
            _serviceScopeFactory,
            _dataMaskerMock.Object,
            _loggerMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _auditLoggerMock.Verify(
            x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCaptureException_WhenExceptionOccurs()
    {
        // Arrange
        var context = CreateHttpContext("/api/test", "GET");
        var exception = new Exception("Test exception");
        var next = new RequestDelegate(_ => throw exception);
        var middleware = new GlobalAuditMiddleware(
            next,
            _serviceScopeFactory,
            _dataMaskerMock.Object,
            _loggerMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => middleware.InvokeAsync(context));

        _exceptionCaptureMock.Verify(
            x => x.CaptureExceptionAsync(
                exception,
                It.Is<ExceptionContext>(c => c.RequestPath == "/api/test")),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldTrackSecurityEvent_For401Response()
    {
        // Arrange
        var context = CreateHttpContext("/api/test", "GET");
        context.Response.StatusCode = 401;
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var middleware = new GlobalAuditMiddleware(
            next,
            _serviceScopeFactory,
            _dataMaskerMock.Object,
            _loggerMock.Object);
        
        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        _dataMaskerMock.Setup(x => x.MaskSensitiveData(It.IsAny<Dictionary<string, object?>>()))
            .Returns<Dictionary<string, object?>>(d => d ?? new Dictionary<string, object?>());

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _securityTrackerMock.Verify(
            x => x.TrackSecurityEventAsync(It.Is<SecurityEvent>(e =>
                e.EventType == "AUTHORIZATION" &&
                e.Severity == "WARNING" &&
                e.IsSuccess == false)),
            Times.Once);
    }

    private static HttpContext CreateHttpContext(string path, string method)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = new PathString(path);
        context.Request.Method = method;
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
        return context;
    }
}
