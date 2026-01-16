using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs.Auth;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CanPany.Tests.ControllerTests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILogger<AuthController>> _loggerMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(
            _authServiceMock.Object,
            _userServiceMock.Object,
            _loggerMock.Object);
        
        // Setup default HttpContext
        var claims = new List<Claim> { new Claim("sub", "user123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Login_ShouldReturnSuccess_WhenValidCredentials()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            FullName = "Test User"
        };
        
        _authServiceMock.Setup(x => x.AuthenticateAsync(request.Email, request.Password))
            .ReturnsAsync(user);
        _authServiceMock.Setup(x => x.GenerateTokenAsync(user))
            .ReturnsAsync("jwt-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword");
        
        _authServiceMock.Setup(x => x.AuthenticateAsync(request.Email, request.Password))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(unauthorizedResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Logout_ShouldReturnSuccess_WhenAuthenticated()
    {
        // Arrange
        var userId = "user123";
        var claims = new List<Claim> { new Claim("sub", userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;
        httpContext.Request.Headers["Authorization"] = "Bearer token123";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        _authServiceMock.Setup(x => x.LogoutAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var userId = "user123";
        var claims = new List<Claim> { new Claim("sub", userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        var request = new ChangePasswordRequest("OldPass123!", "NewPass123!");
        
        _userServiceMock.Setup(x => x.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword))
            .ReturnsAsync((true, Array.Empty<string>()));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnBadRequest_WhenInvalidOldPassword()
    {
        // Arrange
        var userId = "user123";
        var claims = new List<Claim> { new Claim("sub", userId) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        var request = new ChangePasswordRequest("WrongPassword", "NewPass123!");
        
        _userServiceMock.Setup(x => x.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword))
            .ReturnsAsync((false, new[] { "Old password is incorrect" }));

        // Act
        var result = await _controller.ChangePassword(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}
