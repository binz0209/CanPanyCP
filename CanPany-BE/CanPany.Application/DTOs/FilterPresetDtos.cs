using CanPany.Domain.Enums;

namespace CanPany.Application.DTOs;

public class FilterPresetDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFilterPresetDto
{
    public string Name { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class UpdateFilterPresetDto
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Filters { get; set; }
}
