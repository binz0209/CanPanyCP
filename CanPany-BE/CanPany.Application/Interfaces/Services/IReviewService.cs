using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for Review management
/// </summary>
public interface IReviewService
{
    Task<Review?> GetByIdAsync(string id);
    Task<IEnumerable<Review>> GetByContractIdAsync(string contractId);
    Task<IEnumerable<Review>> GetByRevieweeIdAsync(string userId);
    Task<Review> CreateAsync(string contractId, string reviewerId, string revieweeId, int rating, string? comment);
}
