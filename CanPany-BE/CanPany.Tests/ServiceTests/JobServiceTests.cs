using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _repoMock = new();
    private readonly Mock<IJobMatchingService> _matchingServiceMock = new();
    private readonly Mock<ILogger<JobService>> _loggerMock = new();
    private readonly JobService _service;

    public JobServiceTests()
    {
        _service = new JobService(_repoMock.Object, _matchingServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnJob_WhenExists()
    {
        // Arrange
        var job = new Job { Id = "j1", Title = "Developer" };
        _repoMock.Setup(x => x.GetByIdAsync("j1")).ReturnsAsync(job);

        // Act
        var result = await _service.GetByIdAsync("j1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Developer", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Job?)null);

        // Act
        var result = await _service.GetByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllJobs()
    {
        // Arrange
        var jobs = new List<Job> { new() { Id = "j1" }, new() { Id = "j2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(jobs);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_ShouldReturnJobs()
    {
        // Arrange
        var jobs = new List<Job> { new() { Id = "j1", CompanyId = "co1" } };
        _repoMock.Setup(x => x.GetByCompanyIdAsync("co1")).ReturnsAsync(jobs);

        // Act
        var result = (await _service.GetByCompanyIdAsync("co1")).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_ShouldThrow_WhenEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCompanyIdAsync(""));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnJobs()
    {
        // Arrange
        var jobs = new List<Job> { new() { Id = "j1", Status = "Active" } };
        _repoMock.Setup(x => x.GetByStatusAsync("Active")).ReturnsAsync(jobs);

        // Act
        var result = (await _service.GetByStatusAsync("Active")).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldThrow_WhenStatusEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByStatusAsync(""));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnResults()
    {
        // Arrange
        var jobs = new List<Job> { new() { Id = "j1" } };
        _repoMock.Setup(x => x.SearchAsync("dev", null, null, null, null)).ReturnsAsync(jobs);

        // Act
        var result = (await _service.SearchAsync("dev", null, null, null, null)).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnJob_AndTriggerMatching()
    {
        // Arrange
        var job = new Job { Title = "New Job" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Job>()))
            .ReturnsAsync((Job j) => { j.Id = "j1"; return j; });

        // Act
        var result = await _service.CreateAsync(job);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default, result.CreatedAt);
        _matchingServiceMock.Verify(x => x.TriggerJobAlertMatching("j1"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var job = new Job { Id = "j1" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("j1", job);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new Job()));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        _repoMock.Setup(x => x.DeleteAsync("j1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("j1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(""));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        _repoMock.Setup(x => x.ExistsAsync("j1")).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync("j1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.ExistsAsync("nonexistent")).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ExistsAsync(""));
    }
}
