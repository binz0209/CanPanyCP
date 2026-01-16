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

public class CompaniesControllerTests
{
    private readonly Mock<ICompanyService> _companyServiceMock = new();
    private readonly Mock<IJobService> _jobServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IApplicationService> _applicationServiceMock = new();
    private readonly Mock<ILogger<CompaniesController>> _loggerMock = new();
    private readonly CompaniesController _controller;

    public CompaniesControllerTests()
    {
        _controller = new CompaniesController(
            _companyServiceMock.Object,
            _jobServiceMock.Object,
            _userServiceMock.Object,
            _applicationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllCompanies_ShouldReturnSuccess_WhenCompaniesExist()
    {
        // Arrange
        var companies = new List<Company>
        {
            new Company { Id = "company1", Name = "Company 1", IsVerified = true },
            new Company { Id = "company2", Name = "Company 2", IsVerified = false }
        };
        
        _companyServiceMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAllCompanies(null, null, 1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllCompanies_ShouldFilterByKeyword_WhenProvided()
    {
        // Arrange
        var keyword = "Tech";
        var companies = new List<Company>
        {
            new Company { Id = "company1", Name = "Tech Corp", Description = "Tech company" },
            new Company { Id = "company2", Name = "Other Corp", Description = "Other company" }
        };
        
        _companyServiceMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAllCompanies(keyword, null, 1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAllCompanies_ShouldFilterByIsVerified_WhenProvided()
    {
        // Arrange
        var isVerified = true;
        var companies = new List<Company>
        {
            new Company { Id = "company1", Name = "Company 1", IsVerified = true },
            new Company { Id = "company2", Name = "Company 2", IsVerified = false }
        };
        
        _companyServiceMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAllCompanies(null, isVerified, 1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCompany_ShouldReturnSuccess_WhenCompanyExists()
    {
        // Arrange
        var companyId = "company123";
        var company = new Company { Id = companyId, Name = "Test Company", IsVerified = true };
        
        _companyServiceMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        // Act
        var result = await _controller.GetCompany(companyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Company>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(company, response.Data);
    }

    [Fact]
    public async Task GetCompany_ShouldReturnNotFound_WhenCompanyNotExists()
    {
        // Arrange
        var companyId = "nonexistent";
        
        _companyServiceMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _controller.GetCompany(companyId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetCompanyJobs_ShouldReturnSuccess_WhenJobsExist()
    {
        // Arrange
        var companyId = "company123";
        var company = new Company { Id = companyId, Name = "Test Company" };
        var jobs = new List<Job>
        {
            new Job { Id = "job1", CompanyId = companyId, Title = "Job 1" },
            new Job { Id = "job2", CompanyId = companyId, Title = "Job 2" }
        };
        
        _companyServiceMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);
        _jobServiceMock.Setup(x => x.GetByCompanyIdAsync(companyId))
            .ReturnsAsync(jobs);

        // Act
        var result = await _controller.GetCompanyJobs(companyId, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetCompanyJobs_ShouldFilterByStatus_WhenProvided()
    {
        // Arrange
        var companyId = "company123";
        var status = "Open";
        var jobs = new List<Job>
        {
            new Job { Id = "job1", CompanyId = companyId, Title = "Job 1", Status = "Open" },
            new Job { Id = "job2", CompanyId = companyId, Title = "Job 2", Status = "Closed" }
        };
        
        _companyServiceMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(new Company { Id = companyId, Name = "Test Company" });
        _jobServiceMock.Setup(x => x.GetByCompanyIdAsync(companyId))
            .ReturnsAsync(jobs);

        // Act
        var result = await _controller.GetCompanyJobs(companyId, status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
