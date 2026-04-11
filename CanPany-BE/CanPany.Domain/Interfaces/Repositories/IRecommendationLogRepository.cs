using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for RecommendationLog entity
/// </summary>
public interface IRecommendationLogRepository
{
    Task<RecommendationLog?> GetByIdAsync(string id);
    Task<IEnumerable<RecommendationLog>> GetByUserIdAsync(string userId, int limit = 20);
    Task<RecommendationLog> AddAsync(RecommendationLog log);
    Task<long> GetTotalCountAsync();
}
