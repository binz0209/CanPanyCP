using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs.FilterPresets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CanPany.Api.Controllers;

/// <summary>
/// Filter Presets controller - CRUD for saved filter configurations
/// </summary>
[ApiController]
[Route("api/filter-presets")]
[Authorize]
public class FilterPresetsController : ControllerBase
{
    private readonly IFilterPresetService _filterPresetService;
    private readonly ILogger<FilterPresetsController> _logger;

    public FilterPresetsController(
        IFilterPresetService filterPresetService,
        ILogger<FilterPresetsController> logger)
    {
        _filterPresetService = filterPresetService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/filter-presets - Get all presets for current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var presets = await _filterPresetService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<object>.CreateSuccess(presets));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter presets");
            return StatusCode(500, ApiResponse.CreateError("Failed to get filter presets", "GetFilterPresetsFailed"));
        }
    }

    /// <summary>
    /// GET /api/filter-presets/{id} - Get single preset by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var preset = await _filterPresetService.GetByIdAsync(id);
            if (preset == null || preset.UserId != userId)
                return NotFound(ApiResponse.CreateError("Filter preset not found", "NotFound"));

            return Ok(ApiResponse<object>.CreateSuccess(preset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filter preset: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get filter preset", "GetFilterPresetFailed"));
        }
    }

    /// <summary>
    /// POST /api/filter-presets - Create a new preset
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFilterPresetRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Convert filters object to JSON string
            var filtersJson = request.Filters != null
                ? JsonSerializer.Serialize(request.Filters)
                : "{}";

            var preset = await _filterPresetService.CreateAsync(userId, request.Name, request.FilterType, filtersJson);
            return Ok(ApiResponse<object>.CreateSuccess(preset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating filter preset");
            return StatusCode(500, ApiResponse.CreateError("Failed to create filter preset", "CreateFilterPresetFailed"));
        }
    }

    /// <summary>
    /// PUT /api/filter-presets/{id} - Update name or filters
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFilterPresetRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var filtersJson = request.Filters != null
                ? JsonSerializer.Serialize(request.Filters)
                : null;

            var preset = await _filterPresetService.UpdateAsync(id, userId, request.Name, filtersJson);
            if (preset == null)
                return NotFound(ApiResponse.CreateError("Filter preset not found", "NotFound"));

            return Ok(ApiResponse<object>.CreateSuccess(preset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating filter preset: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to update filter preset", "UpdateFilterPresetFailed"));
        }
    }

    /// <summary>
    /// DELETE /api/filter-presets/{id} - Delete a preset
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var deleted = await _filterPresetService.DeleteAsync(id, userId);
            if (!deleted)
                return NotFound(ApiResponse.CreateError("Filter preset not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Filter preset deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting filter preset: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to delete filter preset", "DeleteFilterPresetFailed"));
        }
    }
}
