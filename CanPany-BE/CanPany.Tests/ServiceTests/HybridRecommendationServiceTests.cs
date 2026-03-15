using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Tests.ServiceTests;

public class HybridRecommendationServiceTests
{
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<IUserProfileRepository> _profileRepoMock = new();
    private readonly Mock<IApplicationRepository> _applicationRepoMock = new();
    private readonly Mock<IGeminiService> _geminiServiceMock = new();
    private readonly Mock<ICollaborativeFilteringService> _cfServiceMock = new();
    private readonly Mock<IInteractionTrackingService> _interactionServiceMock = new();
    private readonly Mock<ICVRepository> _cvRepoMock = new();
    private readonly Mock<IGitHubAnalysisRepository> _githubAnalysisRepoMock = new();
    private readonly Mock<ILogger<HybridRecommendationService>> _loggerMock = new();
    private readonly HybridRecommendationService _service;

    public HybridRecommendationServiceTests()
    {
        _interactionServiceMock
            .Setup(x => x.GetUserInteractionsAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<UserJobInteraction>());

        _applicationRepoMock
            .Setup(x => x.GetByCandidateIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<DomainApplication>());

        _cfServiceMock
            .Setup(x => x.GetCfScoresForJobsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, double>());

        _cvRepoMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<CV>());

        _githubAnalysisRepoMock
            .Setup(x => x.GetLatestByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync((GitHubAnalysisResult?)null);

        _service = new HybridRecommendationService(
            _jobRepoMock.Object,
            _profileRepoMock.Object,
            _applicationRepoMock.Object,
            _geminiServiceMock.Object,
            _cfServiceMock.Object,
            _interactionServiceMock.Object,
            _cvRepoMock.Object,
            _githubAnalysisRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_EmptyUserId_ReturnsEmpty()
    {
        var result = await _service.GetRecommendedJobsAsync("");
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_NoProfile_ReturnsEmpty()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync((UserProfile?)null);

        // Act
        var result = await _service.GetRecommendedJobsAsync("user1");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_NoOpenJobs_ReturnsEmpty()
    {
        // Arrange
        _profileRepoMock.Setup(x => x.GetByUserIdAsync("user1"))
            .ReturnsAsync(new UserProfile { UserId = "user1", Title = "Developer" });
        _jobRepoMock.Setup(x => x.GetByStatusAsync("Open"))
            .ReturnsAsync(Enumerable.Empty<Job>());

        // Act
        var result = await _service.GetRecommendedJobsAsync("user1");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_ExcludesAppliedJobs()
    {
        // Arrange
        var profile = new UserProfile { UserId = "user1", Title = "Developer" };
        _profileRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(profile);

        var jobs = new List<Job>
        {
            new() { Id = "job1", Title = "Job 1", Status = "Open" },
            new() { Id = "job2", Title = "Job 2", Status = "Open" }
        };
        _jobRepoMock.Setup(x => x.GetByStatusAsync("Open")).ReturnsAsync(jobs);

        // user1 already applied to job1
        var applications = new List<DomainApplication>
        {
            new() { JobId = "job1", CandidateId = "user1" }
        };
        _applicationRepoMock.Setup(x => x.GetByCandidateIdAsync("user1")).ReturnsAsync(applications);

        _interactionServiceMock.Setup(x => x.GetUserInteractionCountAsync("user1")).ReturnsAsync(0);
        _geminiServiceMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<double> { 0.1, 0.2, 0.3 });

        _cfServiceMock.Setup(x => x.GetCfScoresForJobsAsync("user1", It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, double> { ["job2"] = 0 });

        // Act
        var result = (await _service.GetRecommendedJobsAsync("user1")).ToList();

        // Assert — job1 should be excluded
        Assert.Single(result);
        Assert.Equal("job2", result[0].Job.Id);
    }

    [Fact]
    public async Task GetRecommendedJobsAsync_ColdStart_UsesPureSemanticScores()
    {
        // Arrange — fewer than 10 interactions → α=1.0 (pure semantic)
        var profile = new UserProfile { UserId = "user1", Title = "React Developer", Bio = "Frontend" };
        _profileRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(profile);

        var jobs = new List<Job>
        {
            new() { Id = "job1", Title = "React Job", Status = "Open",
                     SkillEmbedding = new List<double> { 0.9, 0.1, 0.1 } }
        };
        _jobRepoMock.Setup(x => x.GetByStatusAsync("Open")).ReturnsAsync(jobs);
        _applicationRepoMock.Setup(x => x.GetByCandidateIdAsync("user1"))
            .ReturnsAsync(Enumerable.Empty<DomainApplication>());

        _interactionServiceMock.Setup(x => x.GetUserInteractionCountAsync("user1")).ReturnsAsync(3); // < 10

        _geminiServiceMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<double> { 0.9, 0.1, 0.1 }); // Same as job → high similarity

        _cfServiceMock.Setup(x => x.GetCfScoresForJobsAsync("user1", It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, double> { ["job1"] = 50.0 }); // CF score should be ignored

        // Act
        var result = (await _service.GetRecommendedJobsAsync("user1")).ToList();

        // Assert — with α=1.0, hybrid score = 1.0 * semantic + 0.0 * CF = pure semantic
        Assert.Single(result);
        // Semantic score should dominate since α=1.0 and vectors are identical
        Assert.True(result[0].HybridScore > 90, $"Expected high semantic score, got {result[0].HybridScore}");
    }

    [Theory]
    [InlineData(0, 1.0)]    // No interactions → pure semantic
    [InlineData(5, 1.0)]    // < 10 → pure semantic
    [InlineData(10, 0.7)]   // 10-49 → 70% semantic
    [InlineData(49, 0.7)]
    [InlineData(50, 0.5)]   // 50-99 → 50/50
    [InlineData(99, 0.5)]
    [InlineData(100, 0.3)]  // 100+ → CF-heavy
    [InlineData(500, 0.3)]
    public void CalculateAlpha_ReturnsCorrectValue(long interactionCount, double expectedAlpha)
    {
        var alpha = HybridRecommendationService.CalculateAlpha(interactionCount);
        Assert.Equal(expectedAlpha, alpha);
    }

    [Theory]
    [InlineData(80.0, 60.0, 1.0, 80.0)]   // α=1.0 → pure semantic
    [InlineData(80.0, 60.0, 0.0, 60.0)]   // α=0.0 → pure CF
    [InlineData(80.0, 60.0, 0.5, 70.0)]   // α=0.5 → average
    [InlineData(80.0, 60.0, 0.7, 74.0)]   // α=0.7 → 0.7*80 + 0.3*60 = 56+18 = 74
    public void CalculateHybridScore_ReturnsCorrectFormula(
        double semantic, double cf, double alpha, double expected)
    {
        var score = HybridRecommendationService.CalculateHybridScore(semantic, cf, alpha);
        Assert.Equal(expected, score, precision: 2);
    }
}
