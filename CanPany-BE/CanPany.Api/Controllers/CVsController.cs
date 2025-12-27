using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// CVs controller - UC-CAN-05 to UC-CAN-12
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CVsController : ControllerBase
{
    private readonly ICVService _cvService;
    private readonly ILogger<CVsController> _logger;

    public CVsController(
        ICVService cvService,
        ILogger<CVsController> logger)
    {
        _cvService = cvService;
        _logger = logger;
    }

    /// <summary>
    /// UC-CAN-07: View CV List
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCVs()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cvs = await _cvService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<CV>>.CreateSuccess(cvs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CVs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get CVs", "GetCVsFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-08: View CV Details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCV(string id)
    {
        try
        {
            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            return Ok(ApiResponse<CV>.CreateSuccess(cv));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to get CV", "GetCVFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-06: Upload CV
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadCV([FromForm] UploadCVRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // TODO: Upload file to Cloudinary and get URL
            var cv = new CV
            {
                UserId = userId,
                FileName = request.File.FileName,
                FileUrl = "", // TODO: Set from Cloudinary upload result
                FileSize = request.File.Length,
                MimeType = request.File.ContentType,
                IsDefault = request.IsDefault ?? false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _cvService.CreateAsync(cv);
            return Ok(ApiResponse<CV>.CreateSuccess(created, "CV uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to upload CV", "UploadCVFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-09: Update CV Name
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCV(string id, [FromBody] UpdateCVRequest request)
    {
        try
        {
            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.FileName))
                cv.FileName = request.FileName;

            await _cvService.UpdateAsync(id, cv);
            return Ok(ApiResponse.CreateSuccess("CV updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to update CV", "UpdateCVFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-10: Delete CV
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCV(string id)
    {
        try
        {
            await _cvService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("CV deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete CV", "DeleteCVFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-11: Set Default CV
    /// </summary>
    [HttpPut("{id}/set-default")]
    public async Task<IActionResult> SetDefaultCV(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _cvService.SetAsDefaultAsync(id, userId);
            return Ok(ApiResponse.CreateSuccess("CV set as default successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to set default CV", "SetDefaultCVFailed"));
        }
    }

    /// <summary>
    /// UC-CAN-12: Analyze CV via AI
    /// </summary>
    [HttpPost("{id}/analyze")]
    public async Task<IActionResult> AnalyzeCV(string id)
    {
        try
        {
            // TODO: Implement AI CV analysis using Gemini API
            // This should extract skills, calculate ATS score, etc.
            return Ok(ApiResponse.CreateSuccess("CV analysis started. Results will be available shortly."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV");
            return StatusCode(500, ApiResponse.CreateError("Failed to analyze CV", "AnalyzeCVFailed"));
        }
    }
}

public record UploadCVRequest(IFormFile File, bool? IsDefault = false);
public record UpdateCVRequest(string? FileName);


