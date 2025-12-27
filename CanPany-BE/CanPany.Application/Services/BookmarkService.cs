using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Bookmark service implementation
/// </summary>
public class BookmarkService : IBookmarkService
{
    private readonly IJobBookmarkRepository _bookmarkRepo;
    private readonly IJobRepository _jobRepo;
    private readonly ILogger<BookmarkService> _logger;

    public BookmarkService(
        IJobBookmarkRepository bookmarkRepo,
        IJobRepository jobRepo,
        ILogger<BookmarkService> logger)
    {
        _bookmarkRepo = bookmarkRepo;
        _jobRepo = jobRepo;
        _logger = logger;
    }

    public async Task<bool> BookmarkJobAsync(string userId, string jobId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            var exists = await _bookmarkRepo.ExistsAsync(userId, jobId);
            if (exists)
                return true; // Already bookmarked

            var bookmark = new JobBookmark
            {
                UserId = userId,
                JobId = jobId,
                CreatedAt = DateTime.UtcNow
            };

            await _bookmarkRepo.AddAsync(bookmark);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bookmarking job: {UserId}, {JobId}", userId, jobId);
            throw;
        }
    }

    public async Task<bool> RemoveBookmarkAsync(string userId, string jobId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(jobId))
                throw new ArgumentException("Job ID cannot be null or empty", nameof(jobId));

            var bookmark = await _bookmarkRepo.GetByUserAndJobAsync(userId, jobId);
            if (bookmark == null)
                return false;

            await _bookmarkRepo.DeleteAsync(bookmark.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing bookmark: {UserId}, {JobId}", userId, jobId);
            throw;
        }
    }

    public async Task<bool> IsBookmarkedAsync(string userId, string jobId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jobId))
                return false;

            return await _bookmarkRepo.ExistsAsync(userId, jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bookmark: {UserId}, {JobId}", userId, jobId);
            return false;
        }
    }

    public async Task<IEnumerable<Job>> GetBookmarkedJobsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var bookmarks = await _bookmarkRepo.GetByUserIdAsync(userId);
            var jobIds = bookmarks.Select(b => b.JobId).ToList();

            var jobs = new List<Job>();
            foreach (var jobId in jobIds)
            {
                var job = await _jobRepo.GetByIdAsync(jobId);
                if (job != null)
                    jobs.Add(job);
            }

            return jobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookmarked jobs: {UserId}", userId);
            throw;
        }
    }
}


