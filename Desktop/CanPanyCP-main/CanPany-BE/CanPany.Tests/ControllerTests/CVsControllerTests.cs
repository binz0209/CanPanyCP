using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace CanPany.Tests.ControllerTests;

public class CVsControllerTests
{
    private readonly Mock<ICVService> _cvServiceMock = new();
    private readonly Mock<ILogger<CVsController>> _loggerMock = new();
    private readonly CVsController _controller;

    public CVsControllerTests()
    {
        _controller = new CVsController(_cvServiceMock.Object, _loggerMock.Object);
        
        // Setup authenticated user
        var claims = new List<Claim> { new Claim("sub", "user123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetCVs_ShouldReturnSuccess_WhenUserHasCVs()
    {
        // Arrange
        var userId = "user123";
        var cvs = new List<CV>
        {
            new CV { Id = "cv1", UserId = userId, FileName = "cv1.pdf" },
            new CV { Id = "cv2", UserId = userId, FileName = "cv2.pdf" }
        };
        
        _cvServiceMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(cvs);

        // Act
        var result = await _controller.GetCVs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<CV>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetCVs_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetCVs();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetCV_ShouldReturnSuccess_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        var cv = new CV { Id = cvId, UserId = "user123", FileName = "cv.pdf" };
        
        _cvServiceMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync(cv);

        // Act
        var result = await _controller.GetCV(cvId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CV>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(cv, response.Data);
    }

    [Fact]
    public async Task GetCV_ShouldReturnNotFound_WhenCVNotExists()
    {
        // Arrange
        var cvId = "nonexistent";
        
        _cvServiceMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync((CV?)null);

        // Act
        var result = await _controller.GetCV(cvId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteCV_ShouldReturnSuccess_WhenCVDeleted()
    {
        // Arrange
        var cvId = "cv123";
        
        _cvServiceMock.Setup(x => x.DeleteAsync(cvId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCV(cvId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteCV_ShouldReturnSuccess_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        
        _cvServiceMock.Setup(x => x.DeleteAsync(cvId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCV(cvId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateCV_ShouldReturnSuccess_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        var cv = new CV { Id = cvId, UserId = "user123", FileName = "old-name.pdf" };
        var request = new CanPany.Api.Controllers.UpdateCVRequest("new-name.pdf");
        
        _cvServiceMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync(cv);
        _cvServiceMock.Setup(x => x.UpdateAsync(cvId, It.IsAny<CV>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCV(cvId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateCV_ShouldReturnNotFound_WhenCVNotExists()
    {
        // Arrange
        var cvId = "nonexistent";
        var request = new CanPany.Api.Controllers.UpdateCVRequest("new-name.pdf");
        
        _cvServiceMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync((CV?)null);

        // Act
        var result = await _controller.UpdateCV(cvId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task SetDefaultCV_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var cvId = "cv123";
        var userId = "user123";
        
        _cvServiceMock.Setup(x => x.SetAsDefaultAsync(cvId, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetDefaultCV(cvId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task SetDefaultCV_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        var cvId = "cv123";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.SetDefaultCV(cvId);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task AnalyzeCV_ShouldReturnSuccess()
    {
        // Arrange
        var cvId = "cv123";

        // Act
        var result = await _controller.AnalyzeCV(cvId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
