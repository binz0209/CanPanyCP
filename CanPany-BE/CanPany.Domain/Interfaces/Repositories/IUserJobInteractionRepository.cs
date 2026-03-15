using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for UserJobInteraction entity
/// </summary>
public interface IUserJobInteractionRepository
{
    Task<UserJobInteraction?> GetByIdAsync(string id);
    Task<IEnumerable<UserJobInteraction>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserJobInteraction>> GetByJobIdAsync(string jobId);
    Task<UserJobInteraction?> GetByUserJobAndTypeAsync(string userId, string jobId, InteractionType type);
    Task<UserJobInteraction> AddAsync(UserJobInteraction interaction);
    Task<IEnumerable<UserJobInteraction>> GetAllAsync();
    Task<long> GetCountByUserIdAsync(string userId);
    Task<IEnumerable<string>> GetDistinctUserIdsAsync();
    Task<IEnumerable<string>> GetDistinctJobIdsByUserAsync(string userId);
}
