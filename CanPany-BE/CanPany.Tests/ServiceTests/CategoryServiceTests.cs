using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly Mock<ILogger<CategoryService>> _loggerMock = new();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var category = new Category { Id = "c1", Name = "IT" };
        _repoMock.Setup(x => x.GetByIdAsync("c1")).ReturnsAsync(category);

        // Act
        var result = await _service.GetByIdAsync("c1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IT", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Category?)null);

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
    public async Task GetAllAsync_ShouldReturnCategories()
    {
        // Arrange
        var categories = new List<Category> { new() { Id = "c1" }, new() { Id = "c2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCategory_WithTimestamp()
    {
        // Arrange
        var category = new Category { Name = "Design" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Category>())).ReturnsAsync((Category c) => c);

        // Act
        var result = await _service.CreateAsync(category);

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
        var category = new Category { Name = "Updated" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("c1", category);

        // Assert
        Assert.True(result);
        Assert.Equal("c1", category.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new Category()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCategoryIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("c1", null!));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        _repoMock.Setup(x => x.DeleteAsync("c1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("c1");

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
