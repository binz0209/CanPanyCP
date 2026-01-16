using CanPany.Application.Interfaces.Services;
using CanPany.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.I18nTests;

public class I18nMiddlewareTests
{
    private readonly Mock<II18nService> _i18nServiceMock = new();
    private readonly Mock<ILogger<I18nMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_ShouldDetectLanguage_FromXLanguageHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Language"] = "en";
        
        var middleware = new I18nMiddleware(
            _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _i18nServiceMock.Object);

        // Assert
        _i18nServiceMock.Verify(x => x.SetLanguage("en"), Times.Once);
        Assert.Equal("en", context.Items["I18nLanguage"]);
        Assert.Equal("en", context.Response.Headers["X-Language"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldDetectLanguage_FromAcceptLanguageHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept-Language"] = "vi-VN,vi;q=0.9";
        
        var middleware = new I18nMiddleware(
            _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _i18nServiceMock.Object);

        // Assert
        _i18nServiceMock.Verify(x => x.SetLanguage("vi"), Times.Once);
        Assert.Equal("vi", context.Items["I18nLanguage"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldDetectLanguage_FromQueryParameter()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?lang=en");
        
        var middleware = new I18nMiddleware(
            _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _i18nServiceMock.Object);

        // Assert
        _i18nServiceMock.Verify(x => x.SetLanguage("en"), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldDefaultToVietnamese_WhenNoLanguageDetected()
    {
        // Arrange
        var context = new DefaultHttpContext();
        
        var middleware = new I18nMiddleware(
            _ => Task.CompletedTask,
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _i18nServiceMock.Object);

        // Assert
        _i18nServiceMock.Verify(x => x.SetLanguage("vi"), Times.Once);
        Assert.Equal("vi", context.Items["I18nLanguage"]);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        
        var middleware = new I18nMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _i18nServiceMock.Object);

        // Assert
        Assert.True(nextCalled);
    }
}
