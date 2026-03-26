namespace CanPany.Application.DTOs.FilterPresets;

/// <summary>
/// DTO for updating an existing filter preset
/// </summary>
public class UpdateFilterPresetRequest
{
    public string? Name { get; set; }
    public object? Filters { get; set; }
}
