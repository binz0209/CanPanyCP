using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class BookmarkServiceTests
{
    private readonly Mock<IJobBookmarkRepository> _bookmarkRepoMock = new();
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<ILogger<BookmarkService>> _loggerMock = new();
    private readonly BookmarkService _service;

    public BookmarkServiceTests()
    {
        _service = new BookmarkService(_bookmarkRepoMock.Object, _jobRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task BookmarkJobAsync_ShouldReturnTrue_WhenNewBookmark()
    {
        // Arrange
        _bookmarkRepoMock.Setup(x => x.ExistsAsync("user1", "job1")).ReturnsAsync(false);
        _bookmarkRepoMock.Setup(x => x.AddAsync(It.IsAny<JobBookmark>()))
            .ReturnsAsync(new JobBookmark { Id = "bm1", UserId = "user1", JobId = "job1" });

        // Act
        var result = await _service.BookmarkJobAsync("user1", "job1");

        // Assert
        Assert.True(result);
        _bookmarkRepoMock.Verify(x => x.AddAsync(It.IsAny<JobBookmark>()), Times.Once);
    }

    [Fact]
    public async Task BookmarkJobAsync_ShouldReturnTrue_WhenAlreadyBookmarked()
    {
        // Arrange
        _bookmarkRepoMock.Setup(x => x.ExistsAsync("user1", "job1")).ReturnsAsync(true);

        // Act
        var result = await _service.BookmarkJobAsync("user1", "job1");

        // Assert
        Assert.True(result);
        _bookmarkRepoMock.Verify(x => x.AddAsync(It.IsAny<JobBookmark>()), Times.Never);
    }

    [Fact]
    public async Task BookmarkJobAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.BookmarkJobAsync("", "job1"));
    }

    [Fact]
    public async Task BookmarkJobAsync_ShouldThrow_WhenJobIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.BookmarkJobAsync("user1", ""));
    }

    [Fact]
    public async Task RemoveBookmarkAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var bookmark = new JobBookmark { Id = "bm1", UserId = "user1", JobId = "job1" };
        _bookmarkRepoMock.Setup(x => x.GetByUserAndJobAsync("user1", "job1")).ReturnsAsync(bookmark);

        // Act
        var result = await _service.RemoveBookmarkAsync("user1", "job1");

        // Assert
        Assert.True(result);
        _bookmarkRepoMock.Verify(x => x.DeleteAsync("bm1"), Times.Once);
    }

    [Fact]
    public async Task RemoveBookmarkAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        _bookmarkRepoMock.Setup(x => x.GetByUserAndJobAsync("user1", "job1")).ReturnsAsync((JobBookmark?)null);

        // Act
        var result = await _service.RemoveBookmarkAsync("user1", "job1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveBookmarkAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveBookmarkAsync("", "job1"));
    }

    [Fact]
    public async Task IsBookmarkedAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        _bookmarkRepoMock.Setup(x => x.ExistsAsync("user1", "job1")).ReturnsAsync(true);

        // Act
        var result = await _service.IsBookmarkedAsync("user1", "job1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsBookmarkedAsync_ShouldReturnFalse_WhenEmptyParams()
    {
        // Arrange — empty userId

        // Act
        var result = await _service.IsBookmarkedAsync("", "job1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetBookmarkedJobsAsync_ShouldReturnJobs()
    {
        // Arrange
        var bookmarks = new List<JobBookmark>
        {
            new() { Id = "bm1", UserId = "user1", JobId = "job1" },
            new() { Id = "bm2", UserId = "user1", JobId = "job2" }
        };
        _bookmarkRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(bookmarks);
        _jobRepoMock.Setup(x => x.GetByIdAsync("job1")).ReturnsAsync(new Job { Id = "job1", Title = "Job 1" });
        _jobRepoMock.Setup(x => x.GetByIdAsync("job2")).ReturnsAsync(new Job { Id = "job2", Title = "Job 2" });

        // Act
        var result = (await _service.GetBookmarkedJobsAsync("user1")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBookmarkedJobsAsync_ShouldSkipDeletedJobs()
    {
        // Arrange
        var bookmarks = new List<JobBookmark>
        {
            new() { Id = "bm1", UserId = "user1", JobId = "job1" },
            new() { Id = "bm2", UserId = "user1", JobId = "job_deleted" }
        };
        _bookmarkRepoMock.Setup(x => x.GetByUserIdAsync("user1")).ReturnsAsync(bookmarks);
        _jobRepoMock.Setup(x => x.GetByIdAsync("job1")).ReturnsAsync(new Job { Id = "job1" });
        _jobRepoMock.Setup(x => x.GetByIdAsync("job_deleted")).ReturnsAsync((Job?)null);

        // Act
        var result = (await _service.GetBookmarkedJobsAsync("user1")).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetBookmarkedJobsAsync_ShouldThrow_WhenUserIdEmpty()
    {
        // Arrange — no setup needed

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetBookmarkedJobsAsync(""));
    }
}
