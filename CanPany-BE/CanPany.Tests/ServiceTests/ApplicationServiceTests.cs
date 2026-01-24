using CanPany.Application.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Tests.ServiceTests;

public class ApplicationServiceTests
{
    private readonly Mock<IApplicationRepository> _repositoryMock = new();
    private readonly Mock<IJobService> _jobServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IBackgroundEmailService> _backgroundEmailServiceMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ILogger<ApplicationService>> _loggerMock = new();
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _service = new ApplicationService(
            _repositoryMock.Object, 
            _jobServiceMock.Object,
            _userServiceMock.Object,
            _backgroundEmailServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnApplication_WhenExists()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication
        {
            Id = applicationId,
            JobId = "job123",
            CandidateId = "candidate123",
            Status = "Pending"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application);

        // Act
        var result = await _service.GetByIdAsync(applicationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicationId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var applicationId = "nonexistent";
        _repositoryMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync((DomainApplication?)null);

        // Act
        var result = await _service.GetByIdAsync(applicationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetByJobIdAsync_ShouldReturnApplications_WhenExist()
    {
        // Arrange
        var jobId = "job123";
        var applications = new List<DomainApplication>
        {
            new DomainApplication { Id = "app1", JobId = jobId, CandidateId = "candidate1" },
            new DomainApplication { Id = "app2", JobId = jobId, CandidateId = "candidate2" }
        };
        
        _repositoryMock.Setup(x => x.GetByJobIdAsync(jobId))
            .ReturnsAsync(applications);

        // Act
        var result = await _service.GetByJobIdAsync(jobId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByCandidateIdAsync_ShouldReturnApplications_WhenExist()
    {
        // Arrange
        var candidateId = "candidate123";
        var applications = new List<DomainApplication>
        {
            new DomainApplication { Id = "app1", CandidateId = candidateId, JobId = "job1" },
            new DomainApplication { Id = "app2", CandidateId = candidateId, JobId = "job2" }
        };
        
        _repositoryMock.Setup(x => x.GetByCandidateIdAsync(candidateId))
            .ReturnsAsync(applications);

        // Act
        var result = await _service.GetByCandidateIdAsync(candidateId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedApplication_WhenValid()
    {
        // Arrange
        var application = new DomainApplication
        {
            JobId = "job123",
            CandidateId = "candidate123",
            CVId = "cv123",
            CoverLetter = "Cover letter text",
            Status = "Pending"
        };
        var createdApplication = new DomainApplication
        {
            Id = "app123",
            JobId = application.JobId,
            CandidateId = application.CandidateId,
            Status = "Pending"
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<DomainApplication>()))
            .ReturnsAsync(createdApplication);

        // Act
        var result = await _service.CreateAsync(application);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenApplicationIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var applicationId = "app123";
        var application = new DomainApplication
        {
            Id = applicationId,
            JobId = "job123",
            CandidateId = "candidate123",
            Status = "Accepted"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(applicationId))
            .ReturnsAsync(application); // Return same app or different to simulate no change, here just Valid flow

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<DomainApplication>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(applicationId, application);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Arrange
        var application = new DomainApplication { Id = "app123" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", application));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var applicationId = "app123";
        _repositoryMock.Setup(x => x.DeleteAsync(applicationId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(applicationId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAppliedAsync_ShouldReturnTrue_WhenAlreadyApplied()
    {
        // Arrange
        var jobId = "job123";
        var candidateId = "candidate123";
        
        _repositoryMock.Setup(x => x.HasAppliedAsync(jobId, candidateId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.HasAppliedAsync(jobId, candidateId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasAppliedAsync_ShouldReturnFalse_WhenNotApplied()
    {
        // Arrange
        var jobId = "job123";
        var candidateId = "candidate123";
        
        _repositoryMock.Setup(x => x.HasAppliedAsync(jobId, candidateId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.HasAppliedAsync(jobId, candidateId);

        // Assert
        Assert.False(result);
    }
}
