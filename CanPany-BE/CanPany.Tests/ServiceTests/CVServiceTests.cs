using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class CVServiceTests
{
    private readonly Mock<ICVRepository> _cvRepositoryMock = new();
    private readonly Mock<ILogger<CVService>> _loggerMock = new();
    private readonly CVService _cvService;

    public CVServiceTests()
    {
        _cvService = new CVService(_cvRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCV_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        var cv = new CV { Id = cvId, UserId = "user123", FileName = "cv.pdf" };
        
        _cvRepositoryMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync(cv);

        // Act
        var result = await _cvService.GetByIdAsync(cvId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cvId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCVNotExists()
    {
        // Arrange
        var cvId = "nonexistent";
        
        _cvRepositoryMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync((CV?)null);

        // Act
        var result = await _cvService.GetByIdAsync(cvId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnCVs_WhenCVsExist()
    {
        // Arrange
        var userId = "user123";
        var cvs = new List<CV>
        {
            new CV { Id = "cv1", UserId = userId, FileName = "cv1.pdf" },
            new CV { Id = "cv2", UserId = userId, FileName = "cv2.pdf" }
        };
        
        _cvRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(cvs);

        // Act
        var result = await _cvService.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCV_WhenValid()
    {
        // Arrange
        var cv = new CV 
        { 
            UserId = "user123", 
            FileName = "cv.pdf",
            FileUrl = "https://example.com/cv.pdf",
            FileSize = 1024,
            MimeType = "application/pdf"
        };
        
        _cvRepositoryMock.Setup(x => x.AddAsync(It.IsAny<CV>()))
            .ReturnsAsync((CV c) => c);

        // Act
        var result = await _cvService.CreateAsync(cv);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cv.FileName, result.FileName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        var cv = new CV { Id = cvId, UserId = "user123", FileName = "updated-cv.pdf" };
        
        _cvRepositoryMock.Setup(x => x.GetByIdAsync(cvId))
            .ReturnsAsync(cv);
        _cvRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<CV>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cvService.UpdateAsync(cvId, cv);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenCVNotExists()
    {
        // Arrange
        var cvId = "nonexistent";
        var cv = new CV { Id = cvId, UserId = "user123", FileName = "cv.pdf" };
        
        _cvRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<CV>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cvService.UpdateAsync(cvId, cv);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenCVExists()
    {
        // Arrange
        var cvId = "cv123";
        
        _cvRepositoryMock.Setup(x => x.DeleteAsync(cvId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cvService.DeleteAsync(cvId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetAsDefaultAsync_ShouldComplete_WhenValid()
    {
        // Arrange
        var cvId = "cv123";
        var userId = "user123";
        
        _cvRepositoryMock.Setup(x => x.SetAsDefaultAsync(cvId, userId))
            .Returns(Task.CompletedTask);

        // Act
        await _cvService.SetAsDefaultAsync(cvId, userId);

        // Assert
        _cvRepositoryMock.Verify(x => x.SetAsDefaultAsync(cvId, userId), Times.Once);
    }
}
