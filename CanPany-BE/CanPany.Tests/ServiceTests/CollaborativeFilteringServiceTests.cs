using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CollaborativeFilteringServiceTests
{
    private readonly Mock<IUserJobInteractionRepository> _interactionRepoMock = new();
    private readonly Mock<ICfModelRepository> _cfModelRepoMock = new();
    private readonly Mock<ILogger<CollaborativeFilteringService>> _loggerMock = new();
    private readonly CollaborativeFilteringService _service;

    public CollaborativeFilteringServiceTests()
    {
        _service = new CollaborativeFilteringService(_interactionRepoMock.Object, _cfModelRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCfScoreAsync_NoInteractions_ReturnsZero()
    {
        // Arrange - cold start: no interactions at all
        _interactionRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<UserJobInteraction>());

        // Act
        var score = await _service.GetCfScoreAsync("user1", "job1");

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public async Task GetCfScoreAsync_UserNotInMatrix_ReturnsZero()
    {
        // Arrange — interactions exist but not for the target user
        var interactions = new List<UserJobInteraction>
        {
            new() { UserId = "other_user", JobId = "job1", Score = 5.0, Type = InteractionType.Apply }
        };
        _interactionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(interactions);

        // Act
        var score = await _service.GetCfScoreAsync("user1", "job1");

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public async Task GetCfScoreAsync_SimilarUsers_ReturnsPositiveScore()
    {
        // Arrange — user1 and user2 both liked job1 and job2, predict user1's score for job3
        // user2 also liked job3, so CF should predict a positive score for user1-job3
        var interactions = new List<UserJobInteraction>
        {
            // user1 interactions
            new() { UserId = "user1", JobId = "job1", Score = 5.0, Type = InteractionType.Apply },
            new() { UserId = "user1", JobId = "job2", Score = 3.0, Type = InteractionType.Bookmark },
            // user2 interactions (similar to user1)
            new() { UserId = "user2", JobId = "job1", Score = 5.0, Type = InteractionType.Apply },
            new() { UserId = "user2", JobId = "job2", Score = 3.0, Type = InteractionType.Bookmark },
            new() { UserId = "user2", JobId = "job3", Score = 4.0, Type = InteractionType.Apply }
        };
        _interactionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(interactions);

        // Act
        var score = await _service.GetCfScoreAsync("user1", "job3");

        // Assert — should predict a positive score since similar user liked job3
        Assert.True(score > 0, $"Expected positive CF score but got {score}");
    }

    [Fact]
    public async Task GetCfScoreAsync_NoNeighborsWithJob_ReturnsZero()
    {
        // Arrange — user1 has interactions but no neighbors rated the target job
        var interactions = new List<UserJobInteraction>
        {
            new() { UserId = "user1", JobId = "job1", Score = 5.0, Type = InteractionType.Apply },
            new() { UserId = "user2", JobId = "job2", Score = 3.0, Type = InteractionType.Bookmark }
        };
        _interactionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(interactions);

        // Act — job3 not rated by anyone
        var score = await _service.GetCfScoreAsync("user1", "job3");

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public async Task GetCfScoresForJobsAsync_BatchScoring_ReturnsAllJobIds()
    {
        // Arrange
        var interactions = new List<UserJobInteraction>
        {
            new() { UserId = "user1", JobId = "job1", Score = 5.0, Type = InteractionType.Apply },
            new() { UserId = "user2", JobId = "job1", Score = 5.0, Type = InteractionType.Apply },
            new() { UserId = "user2", JobId = "job2", Score = 3.0, Type = InteractionType.Bookmark }
        };
        _interactionRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(interactions);

        var jobIds = new[] { "job1", "job2", "job3" };

        // Act
        var scores = await _service.GetCfScoresForJobsAsync("user1", jobIds);

        // Assert — all requested job IDs should be in the result
        Assert.Equal(3, scores.Count);
        Assert.True(scores.ContainsKey("job1"));
        Assert.True(scores.ContainsKey("job2"));
        Assert.True(scores.ContainsKey("job3"));
    }

    [Fact]
    public async Task GetCfScoresForJobsAsync_EmptyInteractions_ReturnsAllZeros()
    {
        // Arrange
        _interactionRepoMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<UserJobInteraction>());

        var jobIds = new[] { "job1", "job2" };

        // Act
        var scores = await _service.GetCfScoresForJobsAsync("user1", jobIds);

        // Assert
        Assert.Equal(2, scores.Count);
        Assert.Equal(0, scores["job1"]);
        Assert.Equal(0, scores["job2"]);
    }
}
