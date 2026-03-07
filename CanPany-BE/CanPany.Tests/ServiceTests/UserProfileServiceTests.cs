using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileRepository> _repoMock = new();
    private readonly Mock<IGitHubAnalysisRepository> _githubAnalysisRepoMock = new();
    private readonly Mock<IGeminiService> _geminiServiceMock = new();
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ILogger<UserProfileService>> _loggerMock = new();
    private readonly UserProfileService _service;

    public UserProfileServiceTests()
    {
        // Arrange — shared setup
        _configurationMock.Setup(x => x["Encryption:Key"]).Returns("TestKey32CharactersLong!!");

        _encryptionServiceMock.Setup(x => x.Encrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((text, key) => $"ENC:{text}");
        _encryptionServiceMock.Setup(x => x.Decrypt(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((text, key) => text.StartsWith("ENC:") ? text[4..] : text);

        _service = new UserProfileService(
            _repoMock.Object,
            _githubAnalysisRepoMock.Object,
            _geminiServiceMock.Object,
            _encryptionServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnProfile_WithDecryptedPII()
    {
        // Arrange
        var profile = new UserProfile
        {
            Id = "p1",
            UserId = "user1",
            Phone = "ENC:0909123456",
            Address = "ENC:123 Main St"
        };
        _repoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(profile);

        // Act
        var result = await _service.GetByUserIdAsync("user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0909123456", result.Phone);
        Assert.Equal("123 Main St", result.Address);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync((UserProfile?)null);

        // Act
        var result = await _service.GetByUserIdAsync("user1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByUserIdAsync(""));
    }

    [Fact]
    public async Task CreateAsync_ShouldEncryptPII_BeforeSaving()
    {
        // Arrange
        var profile = new UserProfile
        {
            UserId = "user1",
            Phone = "0909123456",
            Address = "123 Main St"
        };
        string? capturedPhone = null;
        string? capturedAddress = null;
        _repoMock.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => { capturedPhone = p.Phone; capturedAddress = p.Address; })
            .ReturnsAsync((UserProfile p) => p);

        // Act
        var result = await _service.CreateAsync(profile);

        // Assert
        Assert.Equal("ENC:0909123456", capturedPhone);
        Assert.Equal("ENC:123 Main St", capturedAddress);
        Assert.Equal("0909123456", result.Phone);
        Assert.Equal("123 Main St", result.Address);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProfileIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldEncryptPII_WhenCreatingNew()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync((UserProfile?)null);
        string? capturedPhone = null;
        _repoMock.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => capturedPhone = p.Phone)
            .ReturnsAsync((UserProfile p) => p);
        var profile = new UserProfile { Phone = "0909111222" };

        // Act
        var result = await _service.UpdateAsync("user1", profile);

        // Assert
        Assert.True(result);
        Assert.Equal("ENC:0909111222", capturedPhone);
    }

    [Fact]
    public async Task UpdateAsync_ShouldEncryptPII_WhenUpdatingExisting()
    {
        // Arrange
        var existing = new UserProfile { Id = "p1", UserId = "user1", CreatedAt = DateTime.UtcNow.AddDays(-1) };
        _repoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(existing);
        string? capturedAddress = null;
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => capturedAddress = p.Address)
            .Returns(Task.CompletedTask);
        var profile = new UserProfile { Address = "456 New St" };

        // Act
        var result = await _service.UpdateAsync("user1", profile);

        // Assert
        Assert.True(result);
        Assert.Equal("ENC:456 New St", capturedAddress);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new UserProfile()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenProfileIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("user1", null!));
    }

    [Fact]
    public async Task CreateAsync_ShouldHandleNullPIIFields()
    {
        // Arrange
        var profile = new UserProfile { UserId = "user1", Phone = null, Address = null };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync((UserProfile p) => p);

        // Act
        var result = await _service.CreateAsync(profile);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Phone);
        Assert.Null(result.Address);
    }
}
