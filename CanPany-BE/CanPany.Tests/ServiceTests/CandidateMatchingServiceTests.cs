using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CandidateMatchingServiceTests
{
    private readonly Mock<ILogger<CandidateMatchingService>> _loggerMock = new();
    private readonly CandidateMatchingService _service;

    public CandidateMatchingServiceTests()
    {
        _service = new CandidateMatchingService(_loggerMock.Object);
    }

    [Fact]
    public void CalculateMatchScore_FullMatch_Returns100()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            SkillIds = new List<string> { "skill1", "skill2" },
            Level = "Senior",
            Location = "Hanoi, Vietnam",
            IsRemote = false
        };

        var profile = new UserProfile
        {
            Id = "profile1",
            SkillIds = new List<string> { "skill1", "skill2", "skill3" },
            Title = "Senior Developer",
            Experience = "Many years of experience",
            Location = "Hanoi, Vietnam"
        };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.Equal(100, result.TotalMatchScore);
        Assert.Equal(100, result.Breakdown.SkillMatch);
        Assert.Equal(100, result.Breakdown.ExperienceMatch);
        Assert.Equal(100, result.Breakdown.LocationMatch);
    }

    [Fact]
    public void CalculateMatchScore_PartialSkillMatch_CalculatesCorrectly()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            SkillIds = new List<string> { "skill1", "skill2", "skill3", "skill4" },
            Level = "Mid",
            Location = "Hanoi, Vietnam"
        };

        var profile = new UserProfile
        {
            Id = "profile1",
            SkillIds = new List<string> { "skill1", "skill2" }, // 50% match
            Title = "Mid Developer",
            Location = "Hanoi, Vietnam"
        };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.Equal(50, result.Breakdown.SkillMatch);
        // Total = 50*0.5 + 100*0.3 + 100*0.2 = 25 + 30 + 20 = 75
        Assert.Equal(75, result.TotalMatchScore);
    }

    [Fact]
    public void CalculateMatchScore_ExperienceLowerByOneLevel_Returns70ForExperience()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            Level = "Senior",
            SkillIds = new List<string>()
        };

        var profile = new UserProfile
        {
            Id = "profile1",
            Title = "Mid Developer",
            Location = job.Location
        };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.Equal(70, result.Breakdown.ExperienceMatch);
    }

    [Fact]
    public void CalculateMatchScore_RemoteJob_Returns100ForLocation()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            IsRemote = true,
            Location = "USA"
        };

        var profile = new UserProfile
        {
            Id = "profile1",
            Location = "Vietnam"
        };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.Equal(100, result.Breakdown.LocationMatch);
    }

    [Fact]
    public void CalculateMatchScore_DifferentCitySameCountry_Returns70ForLocation()
    {
        // Arrange
        var job = new Job
        {
            Id = "job1",
            Location = "Hanoi, Vietnam"
        };

        var profile = new UserProfile
        {
            Id = "profile1",
            Location = "HCM, Vietnam"
        };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.Equal(70, result.Breakdown.LocationMatch);
    }

    [Fact]
    public void CalculateMatchScore_MissingData_HandlesGracefully()
    {
        // Arrange
        var job = new Job { Id = "job1" };
        var profile = new UserProfile { Id = "profile1" };

        // Act
        var result = _service.CalculateMatchScore(job, profile);

        // Assert
        Assert.True(result.TotalMatchScore >= 0);
        Assert.NotNull(result.Breakdown);
    }
}
