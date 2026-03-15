using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CompanyServiceTests
{
    private readonly Mock<ICompanyRepository> _repoMock = new();
    private readonly Mock<ILogger<CompanyService>> _loggerMock = new();
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock = new();
    private readonly CompanyService _service;

    public CompanyServiceTests()
    {
        _service = new CompanyService(_repoMock.Object, _loggerMock.Object, _cloudinaryServiceMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCompany_WhenExists()
    {
        // Arrange
        var company = new Company { Id = "co1", Name = "Acme Corp" };
        _repoMock.Setup(x => x.GetByIdAsync("co1")).ReturnsAsync(company);

        // Act
        var result = await _service.GetByIdAsync("co1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Company?)null);

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
    public async Task GetByUserIdAsync_ShouldReturnCompany_WhenExists()
    {
        // Arrange
        var company = new Company { Id = "co1", UserId = "user1" };
        _repoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(company);

        // Act
        var result = await _service.GetByUserIdAsync("user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user1", result.UserId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByUserIdAsync("user999")).ReturnsAsync((Company?)null);

        // Act
        var result = await _service.GetByUserIdAsync("user999");

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
    public async Task GetAllAsync_ShouldReturnCompanies()
    {
        // Arrange
        var companies = new List<Company> { new() { Id = "co1" }, new() { Id = "co2" } };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(companies);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCompany_WithTimestamp()
    {
        // Arrange
        var company = new Company { Name = "New Corp" };
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Company>())).ReturnsAsync((Company c) => c);

        // Act
        var result = await _service.CreateAsync(company);

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
        var company = new Company { Id = "co1", Name = "Updated" };
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<Company>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync("co1", company);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync("", new Company()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCompanyIsNull()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync("co1", null!));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var company = new Company { Id = "co1", Name = "Acme Corp", CloudinaryPublicId = null };
        _repoMock.Setup(x => x.GetByIdAsync("co1")).ReturnsAsync(company);
        _repoMock.Setup(x => x.DeleteAsync("co1")).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync("co1");

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

    [Fact]
    public async Task DeleteAsync_ShouldCallCloudinaryDelete_WhenCloudinaryPublicIdExists()
    {
        // Arrange
        var companyId = "co1";
        var publicId = "logo_id";
        var company = new Company { Id = companyId, Name = "Acme Corp", CloudinaryPublicId = publicId };
        
        _repoMock.Setup(x => x.GetByIdAsync(companyId)).ReturnsAsync(company);
        _repoMock.Setup(x => x.DeleteAsync(companyId)).Returns(Task.CompletedTask);
        _cloudinaryServiceMock.Setup(x => x.DeleteAsync(publicId, "image")).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(companyId);

        // Assert
        Assert.True(result);
        _cloudinaryServiceMock.Verify(x => x.DeleteAsync(publicId, "image"), Times.Once);
        _repoMock.Verify(x => x.DeleteAsync(companyId), Times.Once);
    }
}
