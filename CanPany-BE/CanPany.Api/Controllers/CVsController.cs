using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Application.Interfaces.Services;
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
    private readonly II18nService _i18nService;
    private readonly IUserPremiumService _userPremiumService;
    private readonly CanPany.Domain.Interfaces.Repositories.IUserRepository _userRepo;

    public CVsController(
        ICVService cvService,
        ILogger<CVsController> logger,
        ICloudinaryService cloudinaryService,
        II18nService i18nService,
        IUserPremiumService userPremiumService,
        CanPany.Domain.Interfaces.Repositories.IUserRepository userRepo)
    {
        _cvService = cvService;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
        _i18nService = i18nService;
        _userPremiumService = userPremiumService;
        _userRepo = userRepo;
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
                    var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.FileRequired);
                    return BadRequest(ApiResponse.CreateError(errorMsg, "FileRequiredOrEmpty"));
                }
                
                _logger.LogInformation("File recovered from Request.Form.Files: {FileName}, Length={Length}", file.FileName, file.Length);
            }
            _logger.LogInformation("=== UploadCV Debug End ===");

            // 1. File Validation
            // Size limit: 5MB
            if (file.Length > 5 * 1024 * 1024)
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.FileTooLarge);
                return BadRequest(ApiResponse.CreateError(errorMsg, "FileTooLarge"));
            }

            // Type validation
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                var errorMsg = _i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.InvalidFileType);
                return BadRequest(ApiResponse.CreateError(errorMsg, "InvalidFileType"));
            }

            // 2. Upload file to Cloudinary
            await using var stream = file.OpenReadStream();
            var resourceType = extension == ".pdf" ? "image" : "raw"; // Upload PDFs as images for inline viewing/transformations
            var (secureUrl, publicId) = await _cloudinaryService.UploadAsync(
                stream,
                file.FileName,
                "cvs",
                resourceType);

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

            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.CV.Uploaded);
            return Ok(ApiResponse<object>.CreateSuccess(responseData, successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.CV.Updated);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.CV.Deleted);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
            
            var successMsg = _i18nService.GetDisplayMessage(I18nKeys.Success.CV.SetDefault);
            return Ok(ApiResponse.CreateSuccess(successMsg));
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
                jobTitle: "backgroundJobs.titles.analyzeCv",
                totalSteps: 100);

            // Update details for i18n parameters
            await progressTracker.UpdateProgressAsync(
                jobId: jobId,
                percentComplete: 0,
                currentStep: "backgroundJobs.steps.pending",
                details: new Dictionary<string, object> { ["fileName"] = cv.FileName ?? "" });
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

    /// <summary>
    /// UC-CAN-13: Generate CV from candidate profile using AI.
    /// Optional: pass ?targetJobId=... to tailor the CV for a specific job posting.
    /// POST /api/cvs/generate
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCV(
        [FromQuery] string? targetJobId,
        [FromServices] CanPany.Worker.Infrastructure.Progress.IJobProgressTracker progressTracker,
        [FromServices] CanPany.Worker.Infrastructure.Queue.IJobProducer jobProducer)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            bool hasPremium = await _userPremiumService.CheckUserPremiumAsync(userId);
            var user = await _userRepo.GetByIdAsync(userId);
            if (!hasPremium)
            {
                if (user != null && user.AiCvGenerationCount >= 2)
                {
                    return BadRequest(ApiResponse.CreateError("You have used up all your free CV creation attempts. Please upgrade to Premium to continue.", "PremiumRequired"));
                }
                if (user != null)
                {
                    user.AiCvGenerationCount += 1;
                    await _userRepo.UpdateAsync(user);
                }
            }

            var jobId = Guid.NewGuid().ToString();

            var jobTitle = string.IsNullOrEmpty(targetJobId)
                ? "backgroundJobs.titles.generateAiCv"
                : "backgroundJobs.titles.tailorAiCv";

            // Initialize progress tracking
            await progressTracker.InitializeAsync(
                jobId: jobId,
                userId: userId,
                jobType: "GenerateCV",
                jobTitle: jobTitle,
                totalSteps: 100);

            // Enqueue job
            var payload = new CanPany.Worker.Models.Payloads.CVGenerationPayload
            {
                UserId   = userId,
                JobTitle = jobTitle,
                JobId    = targetJobId,
            };

            var jobMessage = new CanPany.Worker.Models.JobMessage
            {
                JobId    = jobId,
                I18nKey  = "Job.CV.Generate.Gemini",
                Payload  = System.Text.Json.JsonSerializer.Serialize(payload),
            };

            await jobProducer.EnqueueJobAsync(jobMessage);

            _logger.LogInformation(
                "[CV_GENERATE_QUEUED] JobId: {JobId} | UserId: {UserId} | TargetJobId: {TargetJobId}",
                jobId, userId, targetJobId ?? "none");

            return Ok(ApiResponse<object>.CreateSuccess(
                new { JobId = jobId },
                "CV generation started. Check job status for progress."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting CV generation");
            return StatusCode(500, ApiResponse.CreateError("Failed to start CV generation", "GenerateCVFailed"));
        }
    }

    /// <summary>
    /// GET /api/cvs/{id}/data — return structured CV data for the editor
    /// </summary>
    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetCVData(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null || cv.UserId != userId)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            return Ok(ApiResponse<CVStructuredData>.CreateSuccess(
                cv.StructuredData ?? new CVStructuredData { FullName = "", Email = "" },
                "OK"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET_CV_DATA] id={Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get CV data", "GetCVDataFailed"));
        }
    }

    /// <summary>
    /// PUT /api/cvs/{id}/data — save edited structured CV data
    /// </summary>
    [HttpPut("{id}/data")]
    public async Task<IActionResult> UpdateCVData(string id, [FromBody] CVStructuredData data)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null || cv.UserId != userId)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            cv.StructuredData = data;
            cv.UpdatedAt = DateTime.UtcNow;
            await _cvService.UpdateAsync(id, cv);

            return Ok(ApiResponse<object>.CreateSuccess(new { updated = true }, "CV data saved."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UPDATE_CV_DATA] id={Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to save CV data", "UpdateCVDataFailed"));
        }
    }


    /// <summary>
    /// UC-20: Get all versions of a CV.
    /// GET /api/cvs/{id}/versions
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetCVVersions(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null || cv.UserId != userId)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            // Use the CV's own ID or its ParentCvId as the root
            var rootId = cv.ParentCvId ?? cv.Id;
            var versions = await _cvService.GetVersionsAsync(rootId);

            return Ok(ApiResponse<IEnumerable<CV>>.CreateSuccess(versions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GET_CV_VERSIONS] id={Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to get CV versions", "GetVersionsFailed"));
        }
    }

    /// <summary>
    /// UC-20: Save current CV state as a new version.
    /// POST /api/cvs/{id}/save-version
    /// </summary>
    [HttpPost("{id}/save-version")]
    public async Task<IActionResult> SaveCVVersion(string id, [FromBody] SaveVersionRequest? request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null || cv.UserId != userId)
                return NotFound(ApiResponse.CreateError("CV not found", "NotFound"));

            // Use the original CV as the root parent
            var rootId = cv.ParentCvId ?? cv.Id;
            var nextVersion = await _cvService.GetNextVersionAsync(rootId);

            // Clone the CV as a new version
            var newCv = new CV
            {
                UserId = cv.UserId,
                FileName = cv.FileName,
                FileUrl = cv.FileUrl,
                FileSize = cv.FileSize,
                MimeType = cv.MimeType,
                CloudinaryPublicId = cv.CloudinaryPublicId,
                IsDefault = false,
                ExtractedSkills = new List<string>(cv.ExtractedSkills),
                StructuredData = cv.StructuredData,
                IsAIGenerated = cv.IsAIGenerated,
                Version = nextVersion,
                ParentCvId = rootId,
                VersionNote = request?.VersionNote ?? $"Version {nextVersion}",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _cvService.CreateAsync(newCv);

            return Ok(ApiResponse<CV>.CreateSuccess(created, $"CV version {nextVersion} saved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SAVE_CV_VERSION] id={Id}", id);
            return StatusCode(500, ApiResponse.CreateError("Failed to save CV version", "SaveVersionFailed"));
        }
    }
}



public record UpdateCVRequest(string? FileName);

public record SaveVersionRequest(string? VersionNote);

