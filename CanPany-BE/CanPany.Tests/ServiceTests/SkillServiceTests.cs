using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class SkillServiceTests
{
    private readonly Mock<ISkillRepository> _repoMock = new();
    private readonly Mock<ILogger<SkillService>> _loggerMock = new();
    private readonly SkillService _service;

    public SkillServiceTests()
    {
        _service = new SkillService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSkill_WhenExists()
    {
        // Arrange
        var skill = new Skill { Id = "s1", Name = "C#" };
        _repoMock.Setup(x => x.GetByIdAsync("s1")).ReturnsAsync(skill);

        // Act
        var result = await _service.GetByIdAsync("s1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("C#", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Skill?)null);

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
    public async Task GetAllAsync_ShouldReturnAllSkills()
    {
        // Arrange
        var skills = new List<Skill> { new() { Id = "s1" }, new() { Id = "s2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(skills);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ShouldReturnSkills()
    {
        // Arrange
        var skills = new List<Skill> { new() { Id = "s1", CategoryId = "c1" } };
        _repoMock.Setup(x => x.GetByCategoryIdAsync("c1")).ReturnsAsync(skills);

        // Act
        var result = (await _service.GetByCategoryIdAsync("c1")).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ShouldThrow_WhenCategoryIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCategoryIdAsync(""));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedSkill_WithTimestamp()
    {
        // Arrange
        var skill = new Skill { Name = "React" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Skill>())).ReturnsAsync((Skill s) => s);

        // Act
        var result = await _service.CreateAsync(skill);

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
        var skill = new Skill { Name = "Updated" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Skill>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("s1", skill);

        // Assert
        Assert.True(result);
        Assert.Equal("s1", skill.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new Skill()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenSkillIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("s1", null!));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        _repoMock.Setup(x => x.DeleteAsync("s1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("s1");

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
