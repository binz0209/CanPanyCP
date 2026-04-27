using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class JobAlertServiceTests
{
    private readonly Mock<IJobAlertRepository> _alertRepoMock = new();
    private readonly Mock<IJobAlertMatchRepository> _matchRepoMock = new();
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<ISkillRepository> _skillRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<ILogger<JobAlertService>> _loggerMock = new();
    private readonly JobAlertService _service;

    public JobAlertServiceTests()
    {
        _service = new JobAlertService(
            _alertRepoMock.Object,
            _matchRepoMock.Object,
            _jobRepoMock.Object,
            _companyRepoMock.Object,
            _skillRepoMock.Object,
            _categoryRepoMock.Object,
            _loggerMock.Object);
    }

    // ==================== GetUserAlertsAsync ====================

    [Fact]
    public async Task GetUserAlertsAsync_ShouldReturnAlerts()
    {
        // Arrange
        var alerts = new List<JobAlert>
        {
            new() { Id = "a1", UserId = "user1", Title = "Alert 1", IsActive = true },
            new() { Id = "a2", UserId = "user1", Title = "Alert 2", IsActive = false }
        };
        _alertRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(alerts);

        // Act
        var result = (await _service.GetUserAlertsAsync("user1")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ==================== GetAlertByIdAsync ====================

    [Fact]
    public async Task GetAlertByIdAsync_ShouldReturnAlert_WhenOwner()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1", Title = "Test" };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);

        // Act
        var result = await _service.GetAlertByIdAsync("user1", "a1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("a1", result.Id);
    }

    [Fact]
    public async Task GetAlertByIdAsync_ShouldReturnNull_WhenNotOwner()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1" };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);

        // Act
        var result = await _service.GetAlertByIdAsync("otherUser", "a1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAlertByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _alertRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((JobAlert?)null);

        // Act
        var result = await _service.GetAlertByIdAsync("user1", "nonexistent");

        // Assert
        Assert.Null(result);
    }

    // ==================== DeleteAlertAsync ====================

    [Fact]
    public async Task DeleteAlertAsync_ShouldReturnTrue_WhenOwner()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1" };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);
        _alertRepoMock.Setup(x => x.DeleteAsync("a1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAlertAsync("user1", "a1");

        // Assert
        Assert.True(result);
        _alertRepoMock.Verify(x => x.DeleteAsync("a1"), Times.Once);
    }

    [Fact]
    public async Task DeleteAlertAsync_ShouldReturnFalse_WhenNotOwner()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1" };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);

        // Act
        var result = await _service.DeleteAlertAsync("otherUser", "a1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAlertAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        _alertRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((JobAlert?)null);

        // Act
        var result = await _service.DeleteAlertAsync("user1", "nonexistent");

        // Assert
        Assert.False(result);
    }

    // ==================== PauseAlertAsync / ResumeAlertAsync ====================

    [Fact]
    public async Task PauseAlertAsync_ShouldSetInactive()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1", IsActive = true };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);
        _alertRepoMock.Setup(x => x.UpdateAsync(It.IsAny<JobAlert>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.PauseAlertAsync("user1", "a1");

        // Assert
        Assert.True(result);
        Assert.False(alert.IsActive);
    }

    [Fact]
    public async Task ResumeAlertAsync_ShouldSetActive()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", UserId = "user1", IsActive = false };
        _alertRepoMock.Setup(x => x.GetByIdAsync("a1")).ReturnsAsync(alert);
        _alertRepoMock.Setup(x => x.UpdateAsync(It.IsAny<JobAlert>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.ResumeAlertAsync("user1", "a1");

        // Assert
        Assert.True(result);
        Assert.True(alert.IsActive);
    }

    // ==================== FindMatchingAlertsAsync ====================

    [Fact]
    public async Task FindMatchingAlertsAsync_ShouldThrow_WhenJobIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.FindMatchingAlertsAsync(null!));
    }

    [Fact]
    public async Task FindMatchingAlertsAsync_ShouldReturnMatchingAlerts_ByLocation()
    {
        // Arrange
        var alerts = new List<JobAlert>
        {
            new() { Id = "a1", UserId = "u1", IsActive = true, Location = "Ho Chi Minh" },
            new() { Id = "a2", UserId = "u2", IsActive = true, Location = "Da Nang" }
        };
        _alertRepoMock.Setup(x => x.GetActiveAlertsAsync()).ReturnsAsync(alerts);
        _matchRepoMock.Setup(x => x.MatchExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var job = new Job { Id = "j1", Location = "Ho Chi Minh City" };

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
            new() { Id = "a1", IsActive = true, Location = "Da Nang" }
        };
        _alertRepoMock.Setup(x => x.GetActiveAlertsAsync()).ReturnsAsync(alerts);
        _matchRepoMock.Setup(x => x.MatchExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var job = new Job { Id = "j1", Location = "Hanoi" };

        // Act
        var result = (await _service.FindMatchingAlertsAsync(job)).ToList();

        // Assert
        Assert.Empty(result);
    }

    // ==================== FindMatchingJobsAsync ====================

    [Fact]
    public async Task FindMatchingJobsAsync_ShouldReturnMatchingJobs_BySkills()
    {
        // Arrange
        var alert = new JobAlert
        {
            Id = "a1",
            SkillIds = new List<string> { "s1", "s2" }
        };
        var jobs = new List<Job>
        {
            new() { Id = "j1", SkillIds = new List<string> { "s1", "s3" } },
            new() { Id = "j2", SkillIds = new List<string> { "s4" } }
        };
        _matchRepoMock.Setup(x => x.MatchExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = (await _service.FindMatchingJobsAsync(alert, jobs)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("j1", result[0].Id);
    }

    [Fact]
    public async Task FindMatchingJobsAsync_ShouldReturnAll_WhenNoFilters()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1" };
        var jobs = new List<Job>
        {
            new() { Id = "j1" },
            new() { Id = "j2" }
        };
        _matchRepoMock.Setup(x => x.MatchExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = (await _service.FindMatchingJobsAsync(alert, jobs)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FindMatchingJobsAsync_ShouldFilter_ByBudget()
    {
        // Arrange
        var alert = new JobAlert { Id = "a1", MinBudget = 1000000, MaxBudget = 5000000 };
        var jobs = new List<Job>
        {
            new() { Id = "j1", BudgetAmount = 3000000 },
            new() { Id = "j2", BudgetAmount = 500000 },
            new() { Id = "j3", BudgetAmount = 8000000 }
        };
        _matchRepoMock.Setup(x => x.MatchExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = (await _service.FindMatchingJobsAsync(alert, jobs)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("j1", result[0].Id);
    }

    // ==================== GetMatchScoreAsync ====================

    [Fact]
    public async Task GetMatchScoreAsync_ShouldReturnFullScore_WhenAllMatch()
    {
        // Arrange
        var alert = new JobAlert
        {
            Id = "a1",
            SkillIds = new List<string> { "s1" },
            Location = "Ho Chi Minh",
            MinBudget = 1000000,
            MaxBudget = 10000000,
            JobType = "FullTime",
            ExperienceLevel = "Senior"
        };
        var job = new Job
        {
            Id = "j1",
            SkillIds = new List<string> { "s1" },
            Location = "Ho Chi Minh City",
            BudgetAmount = 5000000,
            EngagementType = "FullTime",
            Level = "Senior"
        };

        // Act
        var score = await _service.GetMatchScoreAsync(alert, job);

        // Assert
        Assert.Equal(100, score);
    }

    [Fact]
    public async Task GetMatchScoreAsync_ShouldReturnZero_WhenNothingMatches()
    {
        // Arrange
        var alert = new JobAlert
        {
            Id = "a1",
            SkillIds = new List<string> { "s1" },
            Location = "Da Nang",
            MinBudget = 10000000,
            JobType = "PartTime",
            ExperienceLevel = "Junior"
        };
        var job = new Job
        {
            Id = "j1",
            SkillIds = new List<string> { "s99" },
            Location = "Hanoi",
            BudgetAmount = 500000,
            EngagementType = "FullTime",
            Level = "Senior"
        };

        // Act
        var score = await _service.GetMatchScoreAsync(alert, job);

        // Assert
        Assert.Equal(0, score);
    }
}
