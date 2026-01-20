using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Review entity
/// </summary>
public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(string id);
    Task<IEnumerable<Review>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<Review>> GetByReviewerIdAsync(string reviewerId);
    Task<IEnumerable<Review>> GetByRevieweeIdAsync(string revieweeId);
    Task<Review> AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

