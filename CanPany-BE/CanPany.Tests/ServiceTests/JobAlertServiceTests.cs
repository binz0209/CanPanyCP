using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class JobAlertServiceTests
{
    private readonly Mock<IJobAlertRepository> _repoMock = new();
    private readonly Mock<ILogger<JobAlertService>> _loggerMock = new();
    private readonly JobAlertService _service;

    public JobAlertServiceTests()
    {
        _service = new JobAlertService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetActiveAlertsAsync_ShouldReturnOnlyActiveAlerts()
    {
        // Arrange
        var alerts = new List<JobAlert>
        {
            new() { Id = "a1", IsActive = true },
            new() { Id = "a2", IsActive = false },
            new() { Id = "a3", IsActive = true }
        };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(alerts);

        // Act
        var result = (await _service.GetActiveAlertsAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.IsActive));
    }

    [Fact]
    public async Task GetActiveAlertsAsync_ShouldReturnEmpty_WhenNoActiveAlerts()
    {
        // Arrange
        var alerts = new List<JobAlert> { new() { Id = "a1", IsActive = false } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(alerts);

        // Act
        var result = (await _service.GetActiveAlertsAsync()).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAlertsByUserIdAsync_ShouldReturnAlerts()
    {
        // Arrange
        var alerts = new List<JobAlert> { new() { Id = "a1", UserId = "user1", IsActive = true } };
        _repoMock.Setup(x => x.GetActiveAlertsAsync("user1")).ReturnsAsync(alerts);

        // Act
        var result = (await _service.GetActiveAlertsByUserIdAsync("user1")).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetActiveAlertsByUserIdAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetActiveAlertsByUserIdAsync(""));
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnTrue_WhenAllCriteriaMatch()
    {
        // Arrange
        var job = new Job
        {
            Id = "j1",
            CategoryId = "cat1",
            Location = "Ho Chi Minh",
            IsRemote = true,
            BudgetAmount = 5000000,
            SkillIds = new List<string> { "s1", "s2" }
        };
        var alert = new JobAlert
        {
            Id = "a1",
            CategoryId = "cat1",
            Location = "Ho Chi Minh",
            IsRemote = true,
            MinBudget = 1000000,
            MaxBudget = 10000000,
            SkillIds = new List<string> { "s1" }
        };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnTrue_WhenNoFiltersSet()
    {
        // Arrange
        var job = new Job { Id = "j1", Title = "Dev" };
        var alert = new JobAlert { Id = "a1" };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenCategoryMismatch()
    {
        // Arrange
        var job = new Job { Id = "j1", CategoryId = "cat1" };
        var alert = new JobAlert { Id = "a1", CategoryId = "cat2" };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenLocationMismatch()
    {
        // Arrange
        var job = new Job { Id = "j1", Location = "Ha Noi" };
        var alert = new JobAlert { Id = "a1", Location = "Da Nang" };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenBudgetBelowMin()
    {
        // Arrange
        var job = new Job { Id = "j1", BudgetAmount = 500000 };
        var alert = new JobAlert { Id = "a1", MinBudget = 1000000 };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenBudgetAboveMax()
    {
        // Arrange
        var job = new Job { Id = "j1", BudgetAmount = 15000000 };
        var alert = new JobAlert { Id = "a1", MaxBudget = 10000000 };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenRemoteMismatch()
    {
        // Arrange
        var job = new Job { Id = "j1", IsRemote = false };
        var alert = new JobAlert { Id = "a1", IsRemote = true };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnFalse_WhenNoSkillMatch()
    {
        // Arrange
        var job = new Job { Id = "j1", SkillIds = new List<string> { "s3" } };
        var alert = new JobAlert { Id = "a1", SkillIds = new List<string> { "s1", "s2" } };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckJobMatchesAlert_ShouldReturnTrue_WhenPartialSkillMatch()
    {
        // Arrange
        var job = new Job { Id = "j1", SkillIds = new List<string> { "s1", "s3" } };
        var alert = new JobAlert { Id = "a1", SkillIds = new List<string> { "s1", "s2" } };

        // Act
        var result = _service.CheckJobMatchesAlert(job, alert);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task FindMatchingAlertsAsync_ShouldReturnMatchingAlerts()
    {
        // Arrange
        var alerts = new List<JobAlert>
        {
            new() { Id = "a1", IsActive = true, CategoryId = "cat1" },
            new() { Id = "a2", IsActive = true, CategoryId = "cat2" }
        };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(alerts);
        var job = new Job { Id = "j1", CategoryId = "cat1" };

        // Act
        var result = (await _service.FindMatchingAlertsAsync(job)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("a1", result[0].Id);
    }

    [Fact]
    public async Task FindMatchingAlertsAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        // Arrange
        var alerts = new List<JobAlert>
        {
            new() { Id = "a1", IsActive = true, CategoryId = "cat99" }
        };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(alerts);
        var job = new Job { Id = "j1", CategoryId = "cat1" };

        // Act
        var result = (await _service.FindMatchingAlertsAsync(job)).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task FindMatchingAlertsAsync_ShouldThrow_WhenJobIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.FindMatchingAlertsAsync(null!));
    }
}
