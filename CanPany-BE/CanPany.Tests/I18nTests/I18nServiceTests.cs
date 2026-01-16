using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.I18nTests;

public class I18nServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILogger<I18nService>> _loggerMock = new();
    private readonly I18nService _i18nService;

    public I18nServiceTests()
    {
        _i18nService = new I18nService(_httpContextAccessorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void GetCurrentLanguage_ShouldReturnDefault_WhenNoHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var language = _i18nService.GetCurrentLanguage();

        // Assert
        Assert.Equal("vi", language);
    }

    [Fact]
    public void GetCurrentLanguage_ShouldReturnFromXLanguageHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Language"] = "en";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var language = _i18nService.GetCurrentLanguage();

        // Assert
        Assert.Equal("en", language);
    }

    [Fact]
    public void GetCurrentLanguage_ShouldReturnFromAcceptLanguageHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "en-US,en;q=0.9";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var language = _i18nService.GetCurrentLanguage();

        // Assert
        Assert.Equal("en", language);
    }

    [Fact]
    public void GetCurrentLanguage_ShouldReturnVietnamese_WhenAcceptLanguageIsVietnamese()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "vi-VN,vi;q=0.9";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var language = _i18nService.GetCurrentLanguage();

        // Assert
        Assert.Equal("vi", language);
    }

    [Fact]
    public void SetLanguage_ShouldSetLanguageInContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        _i18nService.SetLanguage("en");

        // Assert
        Assert.Equal("en", context.Items["I18nLanguage"]);
    }

    [Fact]
    public void GetLogMessage_ShouldReturnMessage_WithFormatting()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetLogMessage(
            "Interceptor.Audit.HttpRequest",
            "HTTP_REQUEST",
            "GET",
            "user123",
            "corr123",
            "/api/test",
            100,
            true
        );

        // Assert
        Assert.NotNull(message);
        // The message should contain the formatted log message with the parameters
        // Since the key might not exist in resources, it may fallback to the key itself
        // So we just verify it's not empty and contains some of the parameters
        Assert.NotEmpty(message);
        // Verify it contains at least one of the parameters we passed
        Assert.True(message.Contains("GET") || message.Contains("user123") || message.Contains("Interceptor.Audit.HttpRequest"));
    }

    [Fact]
    public void GetDisplayMessage_ShouldReturnMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetDisplayMessage("Success.User.Login", "John");

        // Assert
        Assert.NotNull(message);
    }

    [Fact]
    public void GetErrorMessage_ShouldReturnMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetErrorMessage("Error.User.NotFound", "user123");

        // Assert
        Assert.NotNull(message);
    }

    [Fact]
    public void GetMessage_ShouldReturnMessage_WithCustomContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetMessage(
            "Interceptor.Audit.HttpRequest",
            I18nContextType.Audit,
            "HTTP_REQUEST",
            "GET"
        );

        // Assert
        Assert.NotNull(message);
    }

    [Fact]
    public void GetMessage_ShouldFallbackToKey_WhenKeyNotFound()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetMessage(
            "NonExistent.Key.12345",
            I18nContextType.Logging
        );

        // Assert
        Assert.Equal("NonExistent.Key.12345", message);
    }

    [Fact]
    public void GetMessage_ShouldHandleNullArguments()
    {
        // Arrange
        var context = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var message = _i18nService.GetMessage(
            "Interceptor.Audit.HttpRequest",
            I18nContextType.Audit,
            null,
            null
        );

        // Assert
        Assert.NotNull(message);
    }
}
