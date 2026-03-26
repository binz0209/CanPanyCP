using CanPany.Domain.Enums;

namespace CanPany.Application.DTOs.FilterPresets;

/// <summary>
/// DTO for creating a new filter preset
/// </summary>
public class CreateFilterPresetRequest
{
    public string Name { get; set; } = string.Empty;
    public FilterPresetType FilterType { get; set; }
    public object? Filters { get; set; }
}
