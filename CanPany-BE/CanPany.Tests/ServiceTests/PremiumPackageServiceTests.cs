using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class PremiumPackageServiceTests
{
    private readonly Mock<IPremiumPackageRepository> _repoMock = new();
    private readonly Mock<ILogger<PremiumPackageService>> _loggerMock = new();
    private readonly PremiumPackageService _service;

    public PremiumPackageServiceTests()
    {
        _service = new PremiumPackageService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPackage_WhenExists()
    {
        // Arrange
        var package = new PremiumPackage { Id = "p1", Name = "Gold" };
        _repoMock.Setup(x => x.GetByIdAsync("p1")).ReturnsAsync(package);

        // Act
        var result = await _service.GetByIdAsync("p1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Gold", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((PremiumPackage?)null);

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
    public async Task GetAllAsync_ShouldReturnAllPackages()
    {
        // Arrange
        var packages = new List<PremiumPackage> { new() { Id = "p1" }, new() { Id = "p2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(packages);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedPackage_WithTimestamp()
    {
        // Arrange
        var package = new PremiumPackage { Name = "Silver" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<PremiumPackage>())).ReturnsAsync((PremiumPackage p) => p);

        // Act
        var result = await _service.CreateAsync(package);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default, result.CreatedAt);
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
        var package = new PremiumPackage { Name = "Updated" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<PremiumPackage>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("p1", package);

        // Assert
        Assert.True(result);
        Assert.Equal("p1", package.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new PremiumPackage()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenPackageIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("p1", null!));
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldReturnTrue_WhenPackageExists()
    {
        // Arrange
        var package = new PremiumPackage { Id = "p1", Price = 100000 };
        _repoMock.Setup(x => x.GetByIdAsync("p1")).ReturnsAsync(package);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<PremiumPackage>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdatePriceAsync("p1", 200000);

        // Assert
        Assert.True(result);
        Assert.Equal(200000, package.Price);
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldReturnFalse_WhenPackageNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("p1")).ReturnsAsync((PremiumPackage?)null);

        // Act
        var result = await _service.UpdatePriceAsync("p1", 200000);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldThrow_WhenPriceIsZero()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdatePriceAsync("p1", 0));
    }

    [Fact]
    public async Task UpdatePriceAsync_ShouldThrow_WhenPriceIsNegative()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdatePriceAsync("p1", -1000));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        _repoMock.Setup(x => x.DeleteAsync("p1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("p1");

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
}
