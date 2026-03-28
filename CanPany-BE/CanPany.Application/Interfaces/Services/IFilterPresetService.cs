using CanPany.Domain.Entities;
using CanPany.Domain.Enums;

namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Service interface for FilterPreset operations
/// </summary>
public interface IFilterPresetService
{
    Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId);
    Task<FilterPreset?> GetByIdAsync(string id);
    Task<FilterPreset> CreateAsync(string userId, string name, FilterPresetType filterType, string filtersJson);
    Task<FilterPreset?> UpdateAsync(string id, string userId, string? name, string? filtersJson);
    Task<bool> DeleteAsync(string id, string userId);
}
