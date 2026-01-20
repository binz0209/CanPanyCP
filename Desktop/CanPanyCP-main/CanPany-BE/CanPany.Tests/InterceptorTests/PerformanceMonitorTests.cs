using CanPany.Application.Interfaces.Interceptors;
using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Interceptors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public class PerformanceMonitorTests
{
    private readonly Mock<ILogger<PerformanceMonitor>> _loggerMock = new();
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly PerformanceMonitor _performanceMonitor;

    public PerformanceMonitorTests()
    {
        _performanceMonitor = new PerformanceMonitor(_loggerMock.Object, _i18nServiceMock.Object);
        
        _i18nServiceMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<I18nContextType>(), It.IsAny<object[]>()))
            .Returns<string, I18nContextType, object[]>((key, context, args) => $"[PERFORMANCE] {key} | {string.Join(" | ", args)}");
    }

    [Fact]
    public void RecordExecutionTime_ShouldUseI18N()
    {
        // Arrange
        var operationName = "TestOperation";
        var milliseconds = 500L;

        // Act
        _performanceMonitor.RecordExecutionTime(operationName, milliseconds);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("OperationCompleted")),
                I18nContextType.Performance,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void RecordExecutionTime_ShouldUseI18N_ForSlowOperation()
    {
        // Arrange
        var operationName = "SlowOperation";
        var milliseconds = 2000L; // > 1000ms threshold

        // Act
        _performanceMonitor.RecordExecutionTime(operationName, milliseconds);

        // Assert
        _i18nServiceMock.Verify(
            x => x.GetMessage(
                It.Is<string>(k => k.Contains("SlowOperation")),
                I18nContextType.Performance,
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void StartTiming_ShouldReturnDisposable()
    {
        // Arrange
        var operationName = "TestOperation";

        // Act
        using var timer = _performanceMonitor.StartTiming(operationName);

        // Assert
        Assert.NotNull(timer);
    }
}
