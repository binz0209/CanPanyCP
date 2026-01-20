using CanPany.Application.Interfaces.Services;
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
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ICompanyService companyService,
        IJobService jobService,
        IUserService userService,
        IApplicationService applicationService,
        ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _jobService = jobService;
        _userService = userService;
        _applicationService = applicationService;
        _logger = logger;
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

            return Ok(ApiResponse.CreateSuccess(result, "Companies retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies");
            return StatusCode(500, ApiResponse.CreateError("Failed to get companies", "GetCompaniesFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            return Ok(ApiResponse<Company>.CreateSuccess(company));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company");
            return StatusCode(500, ApiResponse.CreateError("Failed to get company", "GetCompanyFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.Name)) company.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Description)) company.Description = request.Description;
            if (!string.IsNullOrWhiteSpace(request.LogoUrl)) company.LogoUrl = request.LogoUrl;
            if (!string.IsNullOrWhiteSpace(request.Website)) company.Website = request.Website;
            if (!string.IsNullOrWhiteSpace(request.Phone)) company.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Address)) company.Address = request.Address;

            await _companyService.UpdateAsync(company.Id, company);
            return Ok(ApiResponse.CreateSuccess("Company updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company");
            return StatusCode(500, ApiResponse.CreateError("Failed to update company", "UpdateCompanyFailed"));
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
                return BadRequest(ApiResponse.CreateError("Company already exists for this user", "CompanyExists"));

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
            return Ok(ApiResponse<Company>.CreateSuccess(createdCompany, "Company created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(500, ApiResponse.CreateError("Failed to create company", "CreateCompanyFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            // Only allow if admin or own company
            if (userRole != "Admin" && company.UserId != userId)
                return Forbid();

            var deleted = await _companyService.DeleteAsync(id);
            if (!deleted)
                return BadRequest(ApiResponse.CreateError("Failed to delete company", "DeleteFailed"));

            return Ok(ApiResponse.CreateSuccess("Company deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company");
            return StatusCode(500, ApiResponse.CreateError("Failed to delete company", "DeleteCompanyFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            var jobs = await _jobService.GetByCompanyIdAsync(id);
            
            if (!string.IsNullOrEmpty(status))
            {
                jobs = jobs.Where(j => j.Status == status);
            }

            return Ok(ApiResponse.CreateSuccess(jobs, "Company jobs retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company jobs");
            return StatusCode(500, ApiResponse.CreateError("Failed to get company jobs", "GetCompanyJobsFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

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

            return Ok(ApiResponse.CreateSuccess(statistics, "Statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company statistics");
            return StatusCode(500, ApiResponse.CreateError("Failed to get statistics", "GetStatisticsFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

            company.VerificationStatus = "Pending";
            company.VerificationDocuments = request.DocumentUrls ?? new List<string>();
            await _companyService.UpdateAsync(company.Id, company);

            return Ok(ApiResponse.CreateSuccess("Verification request submitted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting verification");
            return StatusCode(500, ApiResponse.CreateError("Failed to request verification", "RequestVerificationFailed"));
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
                return NotFound(ApiResponse.CreateError("Company not found", "NotFound"));

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

            return Ok(ApiResponse.CreateSuccess(verificationInfo, "Verification status retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status");
            return StatusCode(500, ApiResponse.CreateError("Failed to get verification status", "GetVerificationStatusFailed"));
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

            return Ok(ApiResponse.CreateSuccess(result, "Companies retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching companies");
            return StatusCode(500, ApiResponse.CreateError("Failed to search companies", "SearchCompaniesFailed"));
        }
    }
}

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


