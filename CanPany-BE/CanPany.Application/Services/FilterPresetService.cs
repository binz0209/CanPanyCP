using CanPany.Domain.Entities;
using CanPany.Domain.Enums;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

/// <summary>
/// Filter preset service implementation
/// </summary>
public class FilterPresetService : IFilterPresetService
{
    private readonly IFilterPresetRepository _repo;
    private readonly ILogger<FilterPresetService> _logger;

    public FilterPresetService(
        IFilterPresetRepository repo,
        ILogger<FilterPresetService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<FilterPreset>> GetByUserIdAsync(string userId)
    {
        try
        {
            return await _repo.GetByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter presets for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<FilterPreset?> GetByIdAsync(string id)
    {
        try
        {
            return await _repo.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter preset: {Id}", id);
            throw;
        }
    }

    public async Task<FilterPreset> CreateAsync(string userId, string name, FilterPresetType filterType, string filtersJson)
    {
        try
        {
            var preset = new FilterPreset
            {
                UserId = userId,
                Name = name,
                FilterType = filterType,
                Filters = filtersJson,
                CreatedAt = DateTime.UtcNow
            };

            return await _repo.AddAsync(preset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating filter preset for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<FilterPreset?> UpdateAsync(string id, string userId, string? name, string? filtersJson)
    {
        try
        {
            var preset = await _repo.GetByIdAsync(id);
            if (preset == null || preset.UserId != userId)
                return null;

            if (name != null)
                preset.Name = name;

            if (filtersJson != null)
                preset.Filters = filtersJson;

            await _repo.UpdateAsync(preset);
            return preset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating filter preset: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        try
        {
            var preset = await _repo.GetByIdAsync(id);
            if (preset == null || preset.UserId != userId)
                return false;

            await _repo.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting filter preset: {Id}", id);
            throw;
        }
    }
}
