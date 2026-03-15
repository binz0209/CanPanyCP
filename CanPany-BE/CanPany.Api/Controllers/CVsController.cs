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
    private readonly ICloudinaryService _cloudinaryService;

    public CVsController(
        ICVService cvService,
        ILogger<CVsController> logger,
        ICloudinaryService cloudinaryService)
    {
        _cvService = cvService;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
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
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCV([FromForm] IFormFile file, [FromForm] bool? isDefault = false)
    {
        try
        {
            _logger.LogInformation("=== UploadCV Debug Start ===");
            _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("Content-Length Header: {ContentLength}", Request.ContentLength);
            
            foreach (var header in Request.Headers)
            {
                _logger.LogInformation("Header: {Key}={Value}", header.Key, header.Value);
            }

            _logger.LogInformation("Form files count: {Count}", Request.Form.Files.Count);
            foreach (var f in Request.Form.Files)
            {
                _logger.LogInformation("Found form file: Name={Name}, FileName={FileName}, Length={Length}, ContentType={ContentType}", 
                    f.Name, f.FileName, f.Length, f.ContentType);
            }

            var userId = User.FindFirst("sub")?.Value;

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File parameter is null or empty. Checking Request.Form.Files directly...");
                file = Request.Form.Files.GetFile("file") ?? Request.Form.Files.FirstOrDefault();
                
                if (file == null || file.Length == 0)
                {
                    _logger.LogError("Upload failed: No file content received (file is null or length is 0)");
                    return BadRequest(ApiResponse.CreateError("File is required and must not be empty", "FileRequiredOrEmpty"));
                }
                
                _logger.LogInformation("File recovered from Request.Form.Files: {FileName}, Length={Length}", file.FileName, file.Length);
            }
            _logger.LogInformation("=== UploadCV Debug End ===");

            // 1. File Validation
            // Size limit: 5MB
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiResponse.CreateError("File size exceeds 5MB limit", "FileTooLarge"));

            // Type validation
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse.CreateError("Only PDF and Word documents are allowed", "InvalidFileType"));

            // 2. Upload file to Cloudinary
            await using var stream = file.OpenReadStream();
            var (secureUrl, publicId) = await _cloudinaryService.UploadAsync(
                stream,
                file.FileName,
                "cvs",
                "raw"); // CVs are treated as raw files in Cloudinary

            var cv = new CV
            {
                UserId = userId,
                FileName = file.FileName,
                FileUrl = secureUrl,
                CloudinaryPublicId = publicId,
                FileSize = file.Length,
                MimeType = file.ContentType,
                IsDefault = isDefault ?? false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _cvService.CreateAsync(cv);
            var responseData = new
            {
                Cv = created,
                Url = secureUrl,
                PublicId = publicId
            };

            return Ok(ApiResponse<object>.CreateSuccess(responseData, "CV uploaded successfully"));
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
    public async Task<IActionResult> AnalyzeCV(
        string id,
        [FromServices] CanPany.Worker.Infrastructure.Progress.IJobProgressTracker progressTracker,
        [FromServices] CanPany.Worker.Infrastructure.Queue.IJobProducer jobProducer)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null)
            {
                return NotFound(ApiResponse.CreateError("CV not found", "NOT_FOUND"));
            }

            if (cv.UserId != userId)
            {
                return Forbid();
            }

            // Create a unique Job ID
            var jobId = Guid.NewGuid().ToString();

            // 1. Initialize progress tracking
            await progressTracker.InitializeAsync(
                jobId: jobId,
                userId: userId,
                jobType: "AnalyzeCV",
                jobTitle: $"Phân tích CV: {cv.FileName}",
                totalSteps: 100);

            // 2. Prepare payload
            var payload = new CanPany.Worker.Models.Payloads.CVAnalysisPayload
            {
                UserId = userId,
                CVId = cv.Id
            };

            // 3. Enqueue job
            var jobMessage = new CanPany.Worker.Models.JobMessage
            {
                JobId = jobId,
                I18nKey = "Job.CV.Analyze.Gemini",
                Payload = System.Text.Json.JsonSerializer.Serialize(payload)
            };
            await jobProducer.EnqueueJobAsync(jobMessage);

            _logger.LogInformation(
                "[CV_ANALYSIS_QUEUED] JobId: {JobId} | UserId: {UserId} | CV: {CVId}",
                jobId, userId, id);

            return Ok(ApiResponse<object>.CreateSuccess(new { JobId = jobId }, "CV analysis started. Results will be available shortly."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV: {Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to start CV analysis", "AnalyzeCVFailed"));
        }
    }
}



public record UpdateCVRequest(string? FileName);


