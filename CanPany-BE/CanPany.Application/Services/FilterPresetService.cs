using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Enums;
using CanPany.Domain.Interfaces.Repositories;
using System.Text.Json;

namespace CanPany.Application.Services;

public class FilterPresetService : IFilterPresetService
{
    private readonly IFilterPresetRepository _repository;

    public FilterPresetService(IFilterPresetRepository repository)
    {
        _repository = repository;
    }

    public async Task<FilterPresetDto?> GetByIdAsync(string id)
    {
        var preset = await _repository.GetByIdAsync(id);
        return preset == null ? null : MapToDto(preset);
    }

    public async Task<IEnumerable<FilterPresetDto>> GetByUserIdAsync(string userId, FilterType? type = null)
    {
        IEnumerable<FilterPreset> presets;
        if (type.HasValue)
        {
            presets = await _repository.GetByUserIdAndTypeAsync(userId, type.Value);
        }
        else
        {
            presets = await _repository.GetByUserIdAsync(userId);
        }

        return presets.Select(MapToDto);
    }

    public async Task<FilterPresetDto> CreateAsync(string userId, CreateFilterPresetDto createDto)
    {
        var preset = new FilterPreset
        {
            UserId = userId,
            Name = createDto.Name,
            FilterType = createDto.FilterType,
            Filters = ProcessFilters(createDto.Filters)
        };

        var created = await _repository.AddAsync(preset);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(string id, string userId, UpdateFilterPresetDto updateDto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null || existing.UserId != userId)
        {
            return false;
        }

        if (updateDto.Name != null)
        {
            existing.Name = updateDto.Name;
        }

        if (updateDto.Filters != null)
        {
            existing.Filters = ProcessFilters(updateDto.Filters);
        }

        await _repository.UpdateAsync(existing);
        return true;
    }

    private Dictionary<string, object> ProcessFilters(Dictionary<string, object> filters)
    {
        if (filters == null) return new Dictionary<string, object>();
        
        return filters.ToDictionary(
            p => p.Key,
            p => ConvertValue(p.Value) ?? p.Value
        );
    }

    private object? ConvertValue(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Undefined => null,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(e => ConvertValue(e)).ToList(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertValue(p.Value)),
                _ => element.GetRawText()
            };
        }
        return value;
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null || existing.UserId != userId)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        return true;
    }

    private static FilterPresetDto MapToDto(FilterPreset preset)
    {
        return new FilterPresetDto
        {
            Id = preset.Id,
            UserId = preset.UserId,
            Name = preset.Name,
            FilterType = preset.FilterType,
            Filters = preset.Filters,
            CreatedAt = preset.CreatedAt,
            UpdatedAt = preset.UpdatedAt
        };
    }
}
