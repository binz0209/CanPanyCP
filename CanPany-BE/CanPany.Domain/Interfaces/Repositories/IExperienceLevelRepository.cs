using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for ExperienceLevel entity
/// </summary>
public interface IExperienceLevelRepository
{
    Task<ExperienceLevel?> GetByIdAsync(string id);
    Task<IEnumerable<ExperienceLevel>> GetAllAsync();
    Task<ExperienceLevel> AddAsync(ExperienceLevel level);
    Task UpdateAsync(ExperienceLevel level);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
