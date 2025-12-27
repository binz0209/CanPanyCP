using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Project entity
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(string id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByOwnerIdAsync(string ownerId);
    Task<IEnumerable<Project>> GetByStatusAsync(string status);
    Task<IEnumerable<Project>> SearchAsync(string? keyword, string? categoryId, List<string>? skillIds, decimal? minBudget, decimal? maxBudget);
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}


