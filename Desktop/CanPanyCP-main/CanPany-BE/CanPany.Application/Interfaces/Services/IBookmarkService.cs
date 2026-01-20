using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Bookmark service interface
/// </summary>
public interface IBookmarkService
{
    Task<bool> BookmarkJobAsync(string userId, string jobId);
    Task<bool> RemoveBookmarkAsync(string userId, string jobId);
    Task<bool> IsBookmarkedAsync(string userId, string jobId);
    Task<IEnumerable<Job>> GetBookmarkedJobsAsync(string userId);
}


