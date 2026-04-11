using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Api.Controllers;

/// <summary>
/// Companies controller - UC-CMP-01 to UC-CMP-03 and additional company management APIs
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IJobService _jobService;
    private readonly IUserService _userService;
    private readonly IApplicationService _applicationService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly II18nService _i18nService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ICompanyService companyService,
        IJobService jobService,
        IUserService userService,
        IApplicationService applicationService,
        ICloudinaryService cloudinaryService,
        II18nService i18nService,
        ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _jobService = jobService;
        _userService = userService;
        _applicationService = applicationService;
        _cloudinaryService = cloudinaryService;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// Upload company logo image
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.FileRequired), "FileRequired"));

            // 1. Image Validation
            // Size limit: 2MB
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.FileTooLarge), "FileTooLarge"));

            // Type validation
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.InvalidFileType), "InvalidFileType"));

            // 2. Fetch company to delete old logo if exists
            var company = await _companyService.GetByUserIdAsync(userId);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "CompanyNotFound"));

            if (!string.IsNullOrWhiteSpace(company.CloudinaryPublicId))
            {
                await _cloudinaryService.DeleteAsync(company.CloudinaryPublicId, "image");
            }

            // 3. Upload new image to Cloudinary
            await using var stream = file.OpenReadStream();
            var (secureUrl, publicId) = await _cloudinaryService.UploadAsync(
                stream,
                file.FileName,
                "company-logos",
                "image");

            // 4. Update company entity
            company.LogoUrl = secureUrl;
            company.CloudinaryPublicId = publicId;
            await _companyService.UpdateAsync(company.Id, company);

            return Ok(ApiResponse<object>.CreateSuccess(new { Url = secureUrl, PublicId = publicId }, _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.AvatarUpdated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Profile.Avatar.UploadFailed), "UploadLogoFailed"));
        }
    }

    /// <summary>
    /// Get all companies (with pagination and filters)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCompanies(
        [FromQuery] string? keyword,
        [FromQuery] bool? isVerified,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var companies = await _companyService.GetAllAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                companies = companies.Where(c => 
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (isVerified.HasValue)
            {
                companies = companies.Where(c => c.IsVerified == isVerified.Value);
            }

            // Pagination
            var total = companies.Count();
            var pagedCompanies = companies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Companies = pagedCompanies,
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCompaniesFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-01: View Company Profile
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompany(string id)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            return Ok(ApiResponse<Company>.CreateSuccess(company));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCompanyFailed"));
        }
    }

    /// <summary>
    /// Get my company
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> GetMyCompany()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByUserIdAsync(userId);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            return Ok(ApiResponse<Company>.CreateSuccess(company));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my company");
            return StatusCode(500, ApiResponse.CreateError("Failed to get company", "GetCompanyFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-02: Update Company Profile
    /// </summary>
    [HttpPut("me")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByUserIdAsync(userId);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.Name)) company.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Description)) company.Description = request.Description;
            if (!string.IsNullOrWhiteSpace(request.LogoUrl)) company.LogoUrl = request.LogoUrl;
            if (!string.IsNullOrWhiteSpace(request.Website)) company.Website = request.Website;
            if (!string.IsNullOrWhiteSpace(request.Phone)) company.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Address)) company.Address = request.Address;

            await _companyService.UpdateAsync(company.Id, company);
            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.UpdateFailed), "UpdateCompanyFailed"));
        }
    }

    /// <summary>
    /// Create new company (for registration)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if company already exists
            var existingCompany = await _companyService.GetByUserIdAsync(userId);
            if (existingCompany != null)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "CompanyExists"));

            var company = new Company
            {
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                Phone = request.Phone,
                Address = request.Address,
                IsVerified = false,
                VerificationStatus = "Pending"
            };

            var createdCompany = await _companyService.CreateAsync(company);
            return Ok(ApiResponse<Company>.CreateSuccess(createdCompany, _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateCompanyFailed"));
        }
    }

    /// <summary>
    /// Delete company (soft delete - admin only or own company)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCompany(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            // Only allow if admin or own company
            if (userRole != "Admin" && company.UserId != userId)
                return Forbid();

            var deleted = await _companyService.DeleteAsync(id);
            if (!deleted)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteFailed"));

            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Deleted)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteCompanyFailed"));
        }
    }

    /// <summary>
    /// Get company jobs
    /// </summary>
    [HttpGet("{id}/jobs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanyJobs(string id, [FromQuery] string? status = null)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            var jobs = await _jobService.GetByCompanyIdAsync(id);
            
            if (!string.IsNullOrEmpty(status))
            {
                jobs = jobs.Where(j => j.Status == status);
            }

            return Ok(ApiResponse.CreateSuccess(jobs, _i18nService.GetDisplayMessage(I18nKeys.Success.Application.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company jobs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCompanyJobsFailed"));
        }
    }

    /// <summary>
    /// Get company statistics (own company or admin)
    /// </summary>
    [HttpGet("{id}/statistics")]
    [Authorize]
    public async Task<IActionResult> GetCompanyStatistics(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            // Only allow if admin or own company
            if (userRole != "Admin" && company.UserId != userId)
                return Forbid();

            var jobs = await _jobService.GetByCompanyIdAsync(id);
            var allApplications = new List<DomainApplication>();
            
            foreach (var job in jobs)
            {
                var applications = await _applicationService.GetByJobIdAsync(job.Id);
                allApplications.AddRange(applications);
            }

            var statistics = new
            {
                TotalJobs = jobs.Count(),
                ActiveJobs = jobs.Count(j => j.Status == "Open"),
                ClosedJobs = jobs.Count(j => j.Status == "Closed"),
                DraftJobs = jobs.Count(j => j.Status == "Draft"),
                TotalApplications = allApplications.Count,
                PendingApplications = allApplications.Count(a => a.Status == "Pending"),
                AcceptedApplications = allApplications.Count(a => a.Status == "Accepted"),
                RejectedApplications = allApplications.Count(a => a.Status == "Rejected"),
                TotalViews = jobs.Sum(j => j.ViewCount),
                IsVerified = company.IsVerified,
                VerificationStatus = company.VerificationStatus
            };

            return Ok(ApiResponse.CreateSuccess(statistics, _i18nService.GetDisplayMessage(I18nKeys.Success.Statistics.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company statistics");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetStatisticsFailed"));
        }
    }

    /// <summary>
    /// UC-CMP-03: Create Company Verification Request
    /// </summary>
    [HttpPost("verification")]
    [Authorize(Roles = "Company,Admin")]
    public async Task<IActionResult> RequestVerification([FromBody] VerificationRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByUserIdAsync(userId);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            company.VerificationStatus = "Pending";
            company.VerificationDocuments = request.DocumentUrls ?? new List<string>();
            await _companyService.UpdateAsync(company.Id, company);

            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting verification");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.VerificationFailed), "RequestVerificationFailed"));
        }
    }

    /// <summary>
    /// Get company verification status
    /// </summary>
    [HttpGet("{id}/verification")]
    [Authorize]
    public async Task<IActionResult> GetVerificationStatus(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            // Only allow if admin or own company
            if (userRole != "Admin" && company.UserId != userId)
                return Forbid();

            var verificationInfo = new
            {
                IsVerified = company.IsVerified,
                VerificationStatus = company.VerificationStatus,
                VerifiedAt = company.VerifiedAt,
                VerificationDocuments = company.VerificationDocuments
            };

            return Ok(ApiResponse.CreateSuccess(verificationInfo, _i18nService.GetDisplayMessage(I18nKeys.Success.Statistics.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetVerificationStatusFailed"));
        }
    }

    /// <summary>
    /// Search companies with advanced filters
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchCompanies(
        [FromQuery] string? keyword,
        [FromQuery] bool? isVerified,
        [FromQuery] string? location,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var companies = await _companyService.GetAllAsync();
            
            // Apply filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                companies = companies.Where(c => 
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (isVerified.HasValue)
            {
                companies = companies.Where(c => c.IsVerified == isVerified.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                companies = companies.Where(c => 
                    !string.IsNullOrWhiteSpace(c.Address) && 
                    c.Address.Contains(location, StringComparison.OrdinalIgnoreCase));
            }

            // Pagination
            var total = companies.Count();
            var pagedCompanies = companies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                Companies = pagedCompanies,
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return Ok(ApiResponse.CreateSuccess(result, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching companies");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "SearchCompaniesFailed"));
        }
    }

    /// <summary>
    /// Get all pending company verification requests (Admin only)
    /// </summary>
    [HttpGet("verification-requests")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingVerifications()
    {
        try
        {
            var pendingCompanies = await _companyService.GetPendingVerificationsAsync();
            return Ok(ApiResponse.CreateSuccess(pendingCompanies, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending verifications");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPendingVerificationsFailed"));
        }
    }

    /// <summary>
    /// Add a private note to a company profile (Admin only)
    /// </summary>
    [HttpPost("{id}/note")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddCompanyNote(string id, [FromBody] AddNoteRequest request)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            // Simple implementation: just append to description for now 
            // In a real app, you'd have a separate Notes collection
            var notePrefix = $"\n\n[Admin Note - {DateTime.UtcNow:yyyy-MM-dd}]: ";
            company.Description = (company.Description ?? "") + notePrefix + request.Note;
            
            await _companyService.UpdateAsync(id, company);
            return Ok(ApiResponse.CreateSuccess(_i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to company {CompanyId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "AddNoteFailed"));
        }
    }

    /// <summary>
    /// Trigger AI analysis for a company profile (Admin or Company Owner)
    /// </summary>
    [HttpPost("{id}/analyze")]
    [Authorize]
    public async Task<IActionResult> AnalyzeCompany(string id)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            var userRole = User.FindFirst("role")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            if (userRole != "Admin" && company.UserId != userId)
                return Forbid();

            // Note: Actual AI analysis would be done via a background job similar to GitHub sync.
            // For now, returning accepted status.
            return Accepted(ApiResponse.CreateSuccess(new { JobId = Guid.NewGuid().ToString() }, _i18nService.GetDisplayMessage(I18nKeys.Success.Profile.Updated)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing company {CompanyId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "AnalyzeCompanyFailed"));
        }
    }

    /// <summary>
    /// Get recommended companies for the current user
    /// </summary>
    [HttpGet("recommended")]
    [Authorize]
    public async Task<IActionResult> GetRecommendedCompanies([FromQuery] int limit = 5)
    {
        try
        {
            var recommendedCompanies = await _companyService.GetRecommendedAsync(limit);
            return Ok(ApiResponse.CreateSuccess(recommendedCompanies, _i18nService.GetDisplayMessage(I18nKeys.Success.Candidate.Retrieved)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended companies");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetRecommendedFailed"));
        }
    }
}

public record AddNoteRequest(string Note);

public record UpdateCompanyRequest(
    string? Name = null,
    string? Description = null,
    string? LogoUrl = null,
    string? Website = null,
    string? Phone = null,
    string? Address = null
);

public record CreateCompanyRequest(
    string Name,
    string? Description = null,
    string? LogoUrl = null,
    string? Website = null,
    string? Phone = null,
    string? Address = null
);

public record VerificationRequest(List<string>? DocumentUrls = null);


