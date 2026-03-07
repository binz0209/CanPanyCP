using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CandidateSearchServiceTests
{
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<IUserProfileRepository> _profileRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IApplicationRepository> _applicationRepoMock = new();
    private readonly Mock<ISkillRepository> _skillRepoMock = new();
    private readonly Mock<IGeminiService> _geminiServiceMock = new();
    private readonly Mock<ILogger<CandidateSearchService>> _loggerMock = new();
    private readonly CandidateSearchService _service;

    public CandidateSearchServiceTests()
    {
        _service = new CandidateSearchService(
            _jobRepoMock.Object,
            _profileRepoMock.Object,
            _userRepoMock.Object,
            _applicationRepoMock.Object,
            _skillRepoMock.Object,
            _geminiServiceMock.Object,
            _loggerMock.Object);
    }

    // ==================== SearchCandidatesAsync ====================

    [Fact]
    public async Task SearchCandidatesAsync_ShouldThrow_WhenJobIdEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchCandidatesAsync(""));
    }

    [Fact]
    public async Task SearchCandidatesAsync_ShouldThrow_WhenJobIdNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchCandidatesAsync(null!));
    }

    [Fact]
    public async Task SearchCandidatesAsync_ShouldReturnEmpty_WhenJobNotFound()
    {
        // Arrange
        _jobRepoMock.Setup(x => x.GetByIdAsync("job1")).ReturnsAsync((Job?)null);

        // Act
        var result = await _service.SearchCandidatesAsync("job1");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchCandidatesAsync_ShouldReturnCandidates_WhenJobExists()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            Title = "C# Developer",
            Description = "Looking for a C# developer",
            SkillIds = new List<string> { "skill1" }
        };

        var profiles = new List<(UserProfile Profile, double Score)>
        {
            (new UserProfile { Id = "p1", UserId = "u1", Title = "Dev", SkillIds = new List<string> { "skill1" } }, 0.85)
        };

        var embedding = new List<double> { 0.1, 0.2, 0.3 };

        _jobRepoMock.Setup(x => x.GetByIdAsync("job1")).ReturnsAsync(job);
        _geminiServiceMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(embedding);
        _profileRepoMock.Setup(x => x.SearchByVectorAsync(embedding, 20, 0.5)).ReturnsAsync(profiles);

        // Act
        var result = (await _service.SearchCandidatesAsync("job1")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(85, result[0].MatchScore); // score * 100
    }

    // ==================== UnlockCandidateContactAsync ====================

    [Fact]
    public async Task UnlockCandidateContactAsync_ShouldReturnTrue()
    {
        // Act
        var result = await _service.UnlockCandidateContactAsync("company1", "candidate1");

        // Assert
        Assert.True(result);
    }

    // ==================== HasUnlockedCandidateAsync ====================

    [Fact]
    public async Task HasUnlockedCandidateAsync_ShouldReturnFalse_WhenNotUnlocked()
    {
        // Act
        var result = await _service.HasUnlockedCandidateAsync("company_new", "candidate1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasUnlockedCandidateAsync_ShouldReturnTrue_AfterUnlock()
    {
        // Arrange
        await _service.UnlockCandidateContactAsync("company2", "candidate2");

        // Act
        var result = await _service.HasUnlockedCandidateAsync("company2", "candidate2");

        // Assert
        Assert.True(result);
    }

    // ==================== GetUnlockedCandidatesAsync ====================

    [Fact]
    public async Task GetUnlockedCandidatesAsync_ShouldReturnEmpty_WhenNoUnlocks()
    {
        // Act
        var result = await _service.GetUnlockedCandidatesAsync("company_none");

        // Assert
        Assert.Empty(result);
    }

    // ==================== SemanticSearchAsync ====================

    [Fact]
    public async Task SemanticSearchAsync_ShouldThrow_WhenJobDescriptionEmpty()
    {
        // Arrange
        var request = new SemanticSearchRequest("");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SemanticSearchAsync(request));
    }

    [Fact]
    public async Task SemanticSearchAsync_ShouldReturnResults_WhenValid()
    {
        // Arrange
        var request = new SemanticSearchRequest("C# Developer needed", Limit: 10);
        var embedding = new List<double> { 0.1, 0.2, 0.3 };
        var profile = new UserProfile
        {
            Id = "p1",
            UserId = "u1",
            Title = "C# Dev",
            Bio = "Experienced developer",
            SkillIds = new List<string>()
        };
        var user = new User { Id = "u1", FullName = "Test User", Email = "test@test.com" };

        _geminiServiceMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(embedding);
        _profileRepoMock.Setup(x => x.SearchByVectorAsync(embedding, It.IsAny<int>(), 0.5))
            .ReturnsAsync(new List<(UserProfile, double)> { (profile, 0.9) });
        _userRepoMock.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(user);

        // Act
        var result = (await _service.SemanticSearchAsync(request)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("u1", result[0].Profile.UserId);
    }

    // ==================== SearchCandidatesWithFiltersAsync ====================

    [Fact]
    public async Task SearchCandidatesWithFiltersAsync_ShouldReturnAll_WhenNoKeyword()
    {
        // Arrange
        var profiles = new List<UserProfile>
        {
            new() { Id = "p1", UserId = "u1", Title = "Dev", SkillIds = new List<string>() },
            new() { Id = "p2", UserId = "u2", Title = "Designer", SkillIds = new List<string>() }
        };
        var candidates = new List<User>
        {
            new() { Id = "u1", FullName = "User1", Email = "u1@test.com", Role = "Candidate" },
            new() { Id = "u2", FullName = "User2", Email = "u2@test.com", Role = "Candidate" }
        };

        _profileRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(profiles);
        _userRepoMock.Setup(x => x.GetByRoleAsync("Candidate")).ReturnsAsync(candidates);

        // Act
        var result = (await _service.SearchCandidatesWithFiltersAsync(null, null, null, null, null, null)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchCandidatesWithFiltersAsync_ShouldFilter_ByLocation()
    {
        // Arrange
        var profiles = new List<UserProfile>
        {
            new() { Id = "p1", UserId = "u1", Title = "Dev", Location = "Hanoi", SkillIds = new List<string>() },
            new() { Id = "p2", UserId = "u2", Title = "Dev", Location = "HCMC", SkillIds = new List<string>() }
        };
        var candidates = new List<User>
        {
            new() { Id = "u1", FullName = "User1", Email = "u1@test.com", Role = "Candidate" },
            new() { Id = "u2", FullName = "User2", Email = "u2@test.com", Role = "Candidate" }
        };

        _profileRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(profiles);
        _userRepoMock.Setup(x => x.GetByRoleAsync("Candidate")).ReturnsAsync(candidates);

        // Act
        var result = (await _service.SearchCandidatesWithFiltersAsync(null, null, "Hanoi", null, null, null)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("p1", result[0].Profile.Id);
    }

    [Fact]
    public async Task SearchCandidatesWithFiltersAsync_ShouldFilter_ByHourlyRate()
    {
        // Arrange
        var profiles = new List<UserProfile>
        {
            new() { Id = "p1", UserId = "u1", Title = "Dev", HourlyRate = 50, SkillIds = new List<string>() },
            new() { Id = "p2", UserId = "u2", Title = "Dev", HourlyRate = 100, SkillIds = new List<string>() }
        };
        var candidates = new List<User>
        {
            new() { Id = "u1", FullName = "User1", Email = "u1@test.com", Role = "Candidate" },
            new() { Id = "u2", FullName = "User2", Email = "u2@test.com", Role = "Candidate" }
        };

        _profileRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(profiles);
        _userRepoMock.Setup(x => x.GetByRoleAsync("Candidate")).ReturnsAsync(candidates);

        // Act
        var result = (await _service.SearchCandidatesWithFiltersAsync(null, null, null, null, 40, 60)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(50, result[0].Profile.HourlyRate);
    }
}
