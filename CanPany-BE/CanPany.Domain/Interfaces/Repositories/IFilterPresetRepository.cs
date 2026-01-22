using CanPany.Domain.Entities;
using CanPany.Domain.Enums;

namespace CanPany.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for FilterPreset entity
/// </summary>
public interface IFilterPresetRepository
{
    Task<FilterPreset?> GetByIdAsync(string id);
    Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId);
    Task<IEnumerable<FilterPreset>> GetByUserIdAndTypeAsync(string userId, FilterType filterType);
    Task<FilterPreset> AddAsync(FilterPreset filterPreset);
    Task UpdateAsync(FilterPreset filterPreset);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

