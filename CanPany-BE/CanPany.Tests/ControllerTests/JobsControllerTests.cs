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

public class JobsControllerTests
{
    private readonly Mock<IJobService> _jobServiceMock = new();
    private readonly Mock<IBookmarkService> _bookmarkServiceMock = new();
    private readonly Mock<ILogger<JobsController>> _loggerMock = new();
    private readonly JobsController _controller;

    public JobsControllerTests()
    {
        _controller = new JobsController(
            _jobServiceMock.Object,
            _bookmarkServiceMock.Object,
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
    public async Task SearchJobs_ShouldReturnSuccess_WhenJobsFound()
    {
        // Arrange
        var keyword = "developer";
        var jobs = new List<Job>
        {
            new Job { Id = "job1", Title = "Senior Developer", CompanyId = "company1" },
            new Job { Id = "job2", Title = "Junior Developer", CompanyId = "company2" }
        };
        
        _jobServiceMock.Setup(x => x.SearchAsync(keyword, null, null, null, null))
            .ReturnsAsync(jobs);

        // Act
        var result = await _controller.SearchJobs(keyword, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Job>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task SearchJobs_ShouldReturnEmptyList_WhenNoJobsFound()
    {
        // Arrange
        var keyword = "nonexistent";
        
        _jobServiceMock.Setup(x => x.SearchAsync(keyword, null, null, null, null))
            .ReturnsAsync(new List<Job>());

        // Act
        var result = await _controller.SearchJobs(keyword, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Job>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Empty(response.Data ?? Enumerable.Empty<Job>());
    }

    [Fact]
    public async Task GetJob_ShouldReturnSuccess_WhenJobExists()
    {
        // Arrange
        var jobId = "job123";
        var job = new Job { Id = jobId, Title = "Test Job", CompanyId = "company1" };
        
        _jobServiceMock.Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        _bookmarkServiceMock.Setup(x => x.IsBookmarkedAsync("user123", jobId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetJob(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetJob_ShouldReturnNotFound_WhenJobNotExists()
    {
        // Arrange
        var jobId = "nonexistent";
        
        _jobServiceMock.Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync((Job?)null);

        // Act
        var result = await _controller.GetJob(jobId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetJob_ShouldIncludeBookmarkStatus_WhenUserAuthenticated()
    {
        // Arrange
        var jobId = "job123";
        var job = new Job { Id = jobId, Title = "Test Job", CompanyId = "company1" };
        
        _jobServiceMock.Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        _bookmarkServiceMock.Setup(x => x.IsBookmarkedAsync("user123", jobId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetJob(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetRecommendedJobs_ShouldReturnSuccess_WhenUserAuthenticated()
    {
        // Arrange
        // Controller returns empty list for now (TODO: Implement AI recommendation)

        // Act
        var result = await _controller.GetRecommendedJobs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Job>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Empty(response.Data ?? Enumerable.Empty<Job>());
    }

    [Fact]
    public async Task GetRecommendedJobs_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetRecommendedJobs();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task BookmarkJob_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var jobId = "job123";
        var userId = "user123";
        
        _bookmarkServiceMock.Setup(x => x.BookmarkJobAsync(userId, jobId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.BookmarkJob(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task RemoveBookmark_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var jobId = "job123";
        var userId = "user123";
        
        _bookmarkServiceMock.Setup(x => x.RemoveBookmarkAsync(userId, jobId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveBookmark(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetBookmarkedJobs_ShouldReturnSuccess_WhenUserHasBookmarks()
    {
        // Arrange
        var userId = "user123";
        var jobs = new List<Job>
        {
            new Job { Id = "job1", Title = "Bookmarked Job 1", CompanyId = "company1" },
            new Job { Id = "job2", Title = "Bookmarked Job 2", CompanyId = "company2" }
        };
        
        _bookmarkServiceMock.Setup(x => x.GetBookmarkedJobsAsync(userId))
            .ReturnsAsync(jobs);

        // Act
        var result = await _controller.GetBookmarkedJobs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Job>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }
}
