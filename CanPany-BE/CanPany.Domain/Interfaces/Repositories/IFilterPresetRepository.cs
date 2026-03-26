using CanPany.Domain.Entities;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for FilterPreset entity
/// </summary>
public interface IFilterPresetRepository
{
    Task<FilterPreset?> GetByIdAsync(string id);
    Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId);
    Task<FilterPreset> AddAsync(FilterPreset filterPreset);
    Task UpdateAsync(FilterPreset filterPreset);
    Task DeleteAsync(string id);
}
