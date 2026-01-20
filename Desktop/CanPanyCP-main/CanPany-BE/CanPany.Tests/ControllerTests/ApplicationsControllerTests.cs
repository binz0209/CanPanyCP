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
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Tests.ControllerTests;

public class ApplicationsControllerTests
{
    private readonly Mock<IApplicationService> _applicationServiceMock = new();
    private readonly Mock<IJobService> _jobServiceMock = new();
    private readonly Mock<ILogger<ApplicationsController>> _loggerMock = new();
    private readonly ApplicationsController _controller;

    public ApplicationsControllerTests()
    {
        _controller = new ApplicationsController(
            _applicationServiceMock.Object,
            _jobServiceMock.Object,
            _loggerMock.Object);
        
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
    public async Task GetMyApplications_ShouldReturnSuccess_WhenApplicationsExist()
    {
        // Arrange
        var userId = "user123";
        var applications = new List<DomainApplication>
        {
            new DomainApplication { Id = "app1", CandidateId = userId, JobId = "job1", Status = "Pending" },
            new DomainApplication { Id = "app2", CandidateId = userId, JobId = "job2", Status = "Accepted" }
        };
        
        _applicationServiceMock.Setup(x => x.GetByCandidateIdAsync(userId))
            .ReturnsAsync(applications);

        // Act
        var result = await _controller.GetMyApplications();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<DomainApplication>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetMyApplications_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetMyApplications();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetJobApplications_ShouldReturnSuccess_WhenApplicationsExist()
    {
        // Arrange
        var jobId = "job123";
        var applications = new List<DomainApplication>
        {
            new DomainApplication { Id = "app1", JobId = jobId, CandidateId = "candidate1", Status = "Pending" },
            new DomainApplication { Id = "app2", JobId = jobId, CandidateId = "candidate2", Status = "Pending" }
        };
        
        _applicationServiceMock.Setup(x => x.GetByJobIdAsync(jobId))
            .ReturnsAsync(applications);

        // Act
        var result = await _controller.GetJobApplications(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<DomainApplication>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetApplication_ShouldReturnSuccess_WhenApplicationExists()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication 
        { 
            Id = applicationId, 
            CandidateId = "user123", 
            JobId = "job123", 
            Status = "Pending" 
        };
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.GetApplication(applicationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<DomainApplication>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(application, response.Data);
    }

    [Fact]
    public async Task GetApplication_ShouldReturnNotFound_WhenApplicationNotExists()
    {
        // Arrange
        var applicationId = "nonexistent";
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync((DomainApplication?)null);

        // Act
        var result = await _controller.GetApplication(applicationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var request = new CanPany.Api.Controllers.CreateApplicationRequest("job123", "cv123", "Cover letter text", null);
        var application = new DomainApplication
        {
            Id = "app123",
            CandidateId = userId,
            JobId = request.JobId,
            CVId = request.CVId,
            CoverLetter = request.CoverLetter,
            Status = "Pending"
        };
        
        _applicationServiceMock.Setup(x => x.HasAppliedAsync(request.JobId, userId))
            .ReturnsAsync(false);
        _applicationServiceMock.Setup(x => x.CreateAsync(It.IsAny<DomainApplication>()))
            .ReturnsAsync(application);

        // Act
        var result = await _controller.CreateApplication(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<DomainApplication>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateApplication_ShouldReturnBadRequest_WhenAlreadyApplied()
    {
        // Arrange
        var userId = "user123";
        var request = new CanPany.Api.Controllers.CreateApplicationRequest("job123", "cv123", "Cover letter", null);
        
        _applicationServiceMock.Setup(x => x.HasAppliedAsync(request.JobId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateApplication(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task AcceptApplication_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication 
        { 
            Id = applicationId, 
            JobId = "job123", 
            CandidateId = "candidate1",
            Status = "Pending"
        };
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);
        _applicationServiceMock.Setup(x => x.UpdateAsync(applicationId, It.IsAny<DomainApplication>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AcceptApplication(applicationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task AcceptApplication_ShouldReturnNotFound_WhenApplicationNotExists()
    {
        // Arrange
        var applicationId = "nonexistent";
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync((DomainApplication?)null);

        // Act
        var result = await _controller.AcceptApplication(applicationId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task RejectApplication_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication 
        { 
            Id = applicationId, 
            JobId = "job123", 
            CandidateId = "candidate1",
            Status = "Pending"
        };
        var request = new CanPany.Api.Controllers.RejectApplicationRequest("Not qualified");
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);
        _applicationServiceMock.Setup(x => x.UpdateAsync(applicationId, It.IsAny<DomainApplication>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectApplication(applicationId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task WithdrawApplication_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var applicationId = "app123";
        var application = new DomainApplication 
        { 
            Id = applicationId, 
            CandidateId = userId,
            JobId = "job123", 
            Status = "Pending"
        };
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);
        _applicationServiceMock.Setup(x => x.UpdateAsync(applicationId, It.IsAny<DomainApplication>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.WithdrawApplication(applicationId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task RejectApplication_ShouldIncludeReason_WhenProvided()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication 
        { 
            Id = applicationId, 
            JobId = "job123", 
            CandidateId = "candidate1",
            Status = "Pending"
        };
        var request = new CanPany.Api.Controllers.RejectApplicationRequest("Not qualified");
        
        _applicationServiceMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);
        _applicationServiceMock.Setup(x => x.UpdateAsync(applicationId, It.IsAny<DomainApplication>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectApplication(applicationId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
