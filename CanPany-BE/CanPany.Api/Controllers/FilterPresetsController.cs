using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/filter-presets")]
[Authorize]
public class FilterPresetsController : ControllerBase
{
    private readonly IFilterPresetService _filterPresetService;
    private readonly ILogger<FilterPresetsController> _logger;

    public FilterPresetsController(IFilterPresetService filterPresetService, ILogger<FilterPresetsController> logger)
    {
        _filterPresetService = filterPresetService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPresets([FromQuery] FilterType? type = null)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            //if (string.IsNullOrEmpty(userId)) userId = "6970ee1e165cf77fa781f930";
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var presets = await _filterPresetService.GetByUserIdAsync(userId, type);
            return Ok(ApiResponse.CreateSuccess(presets, "Filter presets retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filter presets");
            return StatusCode(500, ApiResponse.CreateError("Failed to retrieve filter presets", "GetPresetsFailed"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPreset(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var preset = await _filterPresetService.GetByIdAsync(id);
            if (preset == null || preset.UserId != userId)
                return NotFound(ApiResponse.CreateError("Filter preset not found", "NotFound"));

            return Ok(ApiResponse.CreateSuccess(preset, "Filter preset retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filter preset");
            return StatusCode(500, ApiResponse.CreateError("Failed to retrieve filter preset", "GetPresetFailed"));
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePreset([FromBody] CreateFilterPresetDto createDto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var created = await _filterPresetService.CreateAsync(userId, createDto);
            return CreatedAtAction(nameof(GetPreset), new { id = created.Id }, ApiResponse.CreateSuccess(created, "Filter preset created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating filter preset");
            return StatusCode(500, ApiResponse.CreateError("Failed to create filter preset", "CreatePresetFailed"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePreset(string id, [FromBody] UpdateFilterPresetDto updateDto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var succeeded = await _filterPresetService.UpdateAsync(id, userId, updateDto);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Filter preset not found or access denied", "UpdateFailed"));

            return Ok(ApiResponse.CreateSuccess("Filter preset updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating filter preset");
            return StatusCode(500, ApiResponse.CreateError("Failed to update filter preset", "UpdatePresetFailed"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePreset(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var succeeded = await _filterPresetService.DeleteAsync(id, userId);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError("Filter preset not found or access denied", "DeleteFailed"));

            return Ok(ApiResponse.CreateSuccess("Filter preset deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting filter preset");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete filter preset", "DeletePresetFailed"));
        }
    }
}
