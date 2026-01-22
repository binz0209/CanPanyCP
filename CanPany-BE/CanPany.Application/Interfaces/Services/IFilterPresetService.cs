using CanPany.Application.DTOs;
using CanPany.Domain.Enums;

namespace CanPany.Application.Interfaces.Services;

public interface IFilterPresetService
{
    Task<FilterPresetDto?> GetByIdAsync(string id);
    Task<IEnumerable<FilterPresetDto>> GetByUserIdAsync(string userId, FilterType? type = null);
    Task<FilterPresetDto> CreateAsync(string userId, CreateFilterPresetDto createDto);
    Task<bool> UpdateAsync(string id, string userId, UpdateFilterPresetDto updateDto);
    Task<bool> DeleteAsync(string id, string userId);
}
