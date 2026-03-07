using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class BannerServiceTests
{
    private readonly Mock<IBannerRepository> _repoMock = new();
    private readonly Mock<ILogger<BannerService>> _loggerMock = new();
    private readonly BannerService _service;

    public BannerServiceTests()
    {
        _service = new BannerService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBanner_WhenExists()
    {
        // Arrange
        var banner = new Banner { Id = "b1", Title = "Test Banner" };
        _repoMock.Setup(x => x.GetByIdAsync("b1")).ReturnsAsync(banner);

        // Act
        var result = await _service.GetByIdAsync("b1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("b1", result.Id);
        Assert.Equal("Test Banner", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Banner?)null);

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
    public async Task GetAllAsync_ShouldReturnBanners()
    {
        // Arrange
        var banners = new List<Banner> { new() { Id = "b1" }, new() { Id = "b2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(banners);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetActiveBannersAsync_ShouldReturnActiveBanners()
    {
        // Arrange
        var banners = new List<Banner> { new() { Id = "b1", IsActive = true } };
        _repoMock.Setup(x => x.GetActiveBannersAsync()).ReturnsAsync(banners);

        // Act
        var result = (await _service.GetActiveBannersAsync()).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedBanner_WithTimestamp()
    {
        // Arrange
        var banner = new Banner { Title = "New Banner" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Banner>())).ReturnsAsync((Banner b) => b);

        // Act
        var result = await _service.CreateAsync(banner);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default, result.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenBannerIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var banner = new Banner { Title = "Updated" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Banner>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("b1", banner);

        // Assert
        Assert.True(result);
        Assert.Equal("b1", banner.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new Banner()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenBannerIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("b1", null!));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        _repoMock.Setup(x => x.DeleteAsync("b1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("b1");

        // Assert
        Assert.True(result);
        _repoMock.Verify(x => x.DeleteAsync("b1"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(""));
    }
}
