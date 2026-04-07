using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Application.Common.Models;
using CanPany.Application.DTOs;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

/// <summary>
/// Admin controller - UC-ADM-01 to UC-ADM-26
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;
    private readonly IJobService _jobService;
    private readonly IPaymentService _paymentService;
    private readonly ICategoryService _categoryService;
    private readonly ISkillService _skillService;
    private readonly IBannerService _bannerService;
    private readonly IPremiumPackageService _premiumPackageService;
    private readonly II18nService _i18nService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminDashboardService dashboardService,
        IAdminService adminService,
        IUserService userService,
        ICompanyService companyService,
        IJobService jobService,
        IPaymentService paymentService,
        ICategoryService categoryService,
        ISkillService skillService,
        IBannerService bannerService,
        IPremiumPackageService premiumPackageService,
        II18nService i18nService,
        ILogger<AdminController> logger)
    {
        _dashboardService = dashboardService;
        _adminService = adminService;
        _userService = userService;
        _companyService = companyService;
        _jobService = jobService;
        _paymentService = paymentService;
        _categoryService = categoryService;
        _skillService = skillService;
        _bannerService = bannerService;
        _premiumPackageService = premiumPackageService;
        _i18nService = i18nService;
        _logger = logger;
    }

    /// <summary>
    /// UC-ADM-01: View System Dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(ApiResponse<object>.CreateSuccess(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetDashboardFailed"));
        }
    }

    /// <summary>
    /// UC-44: List / Search Users (with filter + pagination)
    /// GET /admin/users?search=&role=&status=&page=&pageSize=
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var users = await _adminService.SearchUsersAsync(search, role, status, page, pageSize);
            return Ok(ApiResponse<IEnumerable<object>>.CreateSuccess(
                users.Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsLocked,
                    u.LockedUntil,
                    u.AvatarUrl,
                    u.CreatedAt
                })));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetUsersFailed"));
        }
    }

    /// <summary>
    /// UC-44: Get single user detail
    /// GET /admin/users/{id}
    /// </summary>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.User.NotFound), "NotFound"));

            return Ok(ApiResponse<object>.CreateSuccess(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.IsLocked,
                user.LockedUntil,
                user.AvatarUrl,
                user.CreatedAt,
                user.UpdatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetUserFailed"));
        }
    }

    /// <summary>
    /// UC-44: Ban User
    /// </summary>
    [HttpPut("users/{id}/ban")]
    public async Task<IActionResult> BanUser(string id)
    {
        try
        {
            var succeeded = await _adminService.BanUserAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.User.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("User banned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error banning user");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "BanUserFailed"));
        }
    }

    /// <summary>
    /// UC-44: Unban User
    /// </summary>
    [HttpPut("users/{id}/unban")]
    public async Task<IActionResult> UnbanUser(string id)
    {
        try
        {
            var succeeded = await _adminService.UnbanUserAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.User.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("User unbanned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unbanning user");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UnbanUserFailed"));
        }
    }

    /// <summary>
    /// UC-44: Permanently delete user from system
    /// DELETE /admin/users/{id}
    /// </summary>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var succeeded = await _adminService.DeleteUserAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.User.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("User permanently deleted"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteUserFailed"));
        }
    }

    /// <summary>
    /// UC-45: Get Verification Requests (fix stub — now queries DB)
    /// GET /admin/companies/verification-requests
    /// </summary>
    [HttpGet("companies/verification-requests")]
    public async Task<IActionResult> GetVerificationRequests()
    {
        try
        {
            var requests = await _adminService.GetVerificationRequestsAsync();
            return Ok(ApiResponse<IEnumerable<Company>>.CreateSuccess(requests));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification requests");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetVerificationRequestsFailed"));
        }
    }

    /// <summary>
    /// UC-45: Get all companies (with optional status filter)
    /// GET /admin/companies?status=
    /// </summary>
    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies([FromQuery] string? status = null)
    {
        try
        {
            var companies = await _adminService.GetAllCompaniesAsync(status);
            return Ok(ApiResponse<IEnumerable<Company>>.CreateSuccess(companies));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCompaniesFailed"));
        }
    }

    /// <summary>
    /// UC-45: Get single company detail
    /// GET /admin/companies/{id}
    /// </summary>
    [HttpGet("companies/{id}")]
    public async Task<IActionResult> GetCompanyById(string id)
    {
        try
        {
            var company = await _adminService.GetCompanyByIdAsync(id);
            if (company == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            return Ok(ApiResponse<Company>.CreateSuccess(company));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company: {CompanyId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCompanyFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-08: Approve Company Verification
    /// </summary>
    [HttpPut("companies/{id}/verify/approve")]
    public async Task<IActionResult> ApproveVerification(string id)
    {
        try
        {
            var succeeded = await _adminService.ApproveCompanyVerificationAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Company verification approved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving verification");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "ApproveVerificationFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-09: Reject Company Verification
    /// </summary>
    [HttpPut("companies/{id}/verify/reject")]
    public async Task<IActionResult> RejectVerification(string id, [FromBody] RejectVerificationRequest request)
    {
        try
        {
            var succeeded = await _adminService.RejectCompanyVerificationAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Company.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Company verification rejected"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting verification");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "RejectVerificationFailed"));
        }
    }

    /// <summary>
    /// UC-46: Get all jobs (admin view — including hidden, with optional status filter)
    /// GET /admin/jobs?status=
    /// </summary>
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs([FromQuery] string? status = null)
    {
        try
        {
            var jobs = await _adminService.GetAllJobsAsync(status);
            return Ok(ApiResponse<IEnumerable<Job>>.CreateSuccess(jobs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting jobs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetJobsFailed"));
        }
    }

    /// <summary>
    /// UC-46: Get single job detail (admin view)
    /// GET /admin/jobs/{id}
    /// </summary>
    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> GetJobById(string id)
    {
        try
        {
            var job = await _adminService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Job.NotFound), "NotFound"));

            return Ok(ApiResponse<Job>.CreateSuccess(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job: {JobId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetJobFailed"));
        }
    }

    /// <summary>
    /// UC-46: Hide Job (Violation)
    /// </summary>
    [HttpPut("jobs/{id}/hide")]
    public async Task<IActionResult> HideJob(string id, [FromBody] HideJobRequest request)
    {
        try
        {
            var succeeded = await _adminService.HideJobAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Job.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Job hidden successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding job");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "HideJobFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-12: Delete Job
    /// </summary>
    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(string id)
    {
        try
        {
            await _adminService.DeleteJobAsync(id);
            return Ok(ApiResponse.CreateSuccess("Job deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteJobFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get all categories
    /// GET /admin/categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Category>>.CreateSuccess(categories));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCategoriesFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get single category detail
    /// GET /admin/categories/{id}
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Category.NotFound), "NotFound"));

            return Ok(ApiResponse<Category>.CreateSuccess(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category: {CategoryId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-13: Create Category
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = new Category { Name = request.Name };
            var created = await _categoryService.CreateAsync(category);
            return Ok(ApiResponse.CreateSuccess(created, "Category created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-14: Update Category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Category.NotFound), "NotFound"));

            category.Name = request.Name;
            await _categoryService.UpdateAsync(id, category);
            return Ok(ApiResponse.CreateSuccess("Category updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UpdateCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-15: Delete Category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Category deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteCategoryFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get all skills (with optional categoryId filter)
    /// GET /admin/skills?categoryId=
    /// </summary>
    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills([FromQuery] string? categoryId = null)
    {
        try
        {
            IEnumerable<Skill> skills = string.IsNullOrWhiteSpace(categoryId)
                ? await _skillService.GetAllAsync()
                : await _skillService.GetByCategoryIdAsync(categoryId);

            return Ok(ApiResponse<IEnumerable<Skill>>.CreateSuccess(skills));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skills");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetSkillsFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get single skill detail
    /// GET /admin/skills/{id}
    /// </summary>
    [HttpGet("skills/{id}")]
    public async Task<IActionResult> GetSkillById(string id)
    {
        try
        {
            var skill = await _skillService.GetByIdAsync(id);
            if (skill == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Skill.NotFound), "NotFound"));

            return Ok(ApiResponse<Skill>.CreateSuccess(skill));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skill: {SkillId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-16: Create Skill
    /// </summary>
    [HttpPost("skills")]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request)
    {
        try
        {
            var skill = new Skill { Name = request.Name, CategoryId = request.CategoryId };
            var created = await _skillService.CreateAsync(skill);
            return Ok(ApiResponse.CreateSuccess(created, "Skill created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-17: Update Skill
    /// </summary>
    [HttpPut("skills/{id}")]
    public async Task<IActionResult> UpdateSkill(string id, [FromBody] UpdateSkillRequest request)
    {
        try
        {
            var skill = await _skillService.GetByIdAsync(id);
            if (skill == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Skill.NotFound), "NotFound"));

            skill.Name = request.Name;
            skill.CategoryId = request.CategoryId;
            await _skillService.UpdateAsync(id, skill);
            return Ok(ApiResponse.CreateSuccess("Skill updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UpdateSkillFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-18: Delete Skill
    /// </summary>
    [HttpDelete("skills/{id}")]
    public async Task<IActionResult> DeleteSkill(string id)
    {
        try
        {
            await _skillService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Skill deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteSkillFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get all banners
    /// GET /admin/banners
    /// </summary>
    [HttpGet("banners")]
    public async Task<IActionResult> GetBanners()
    {
        try
        {
            var banners = await _bannerService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Banner>>.CreateSuccess(banners));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting banners");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetBannersFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get single banner detail
    /// GET /admin/banners/{id}
    /// </summary>
    [HttpGet("banners/{id}")]
    public async Task<IActionResult> GetBannerById(string id)
    {
        try
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Banner.NotFound), "NotFound"));

            return Ok(ApiResponse<Banner>.CreateSuccess(banner));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting banner: {BannerId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-19: Create Banner
    /// </summary>
    [HttpPost("banners")]
    public async Task<IActionResult> CreateBanner([FromBody] CreateBannerRequest request)
    {
        try
        {
            var banner = new Banner
            {
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                LinkUrl = request.LinkUrl,
                Order = request.Order,
                IsActive = request.IsActive ?? true
            };
            var created = await _bannerService.CreateAsync(banner);
            return Ok(ApiResponse.CreateSuccess(created, "Banner created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating banner");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreateBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-20: Update Banner
    /// </summary>
    [HttpPut("banners/{id}")]
    public async Task<IActionResult> UpdateBanner(string id, [FromBody] UpdateBannerRequest request)
    {
        try
        {
            var banner = await _bannerService.GetByIdAsync(id);
            if (banner == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Banner.NotFound), "NotFound"));

            if (!string.IsNullOrWhiteSpace(request.Title)) banner.Title = request.Title;
            if (!string.IsNullOrWhiteSpace(request.ImageUrl)) banner.ImageUrl = request.ImageUrl;
            if (request.LinkUrl != null) banner.LinkUrl = request.LinkUrl;
            if (request.Order.HasValue) banner.Order = request.Order.Value;
            if (request.IsActive.HasValue) banner.IsActive = request.IsActive.Value;

            await _bannerService.UpdateAsync(id, banner);
            return Ok(ApiResponse.CreateSuccess("Banner updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating banner");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UpdateBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-21: Delete Banner
    /// </summary>
    [HttpDelete("banners/{id}")]
    public async Task<IActionResult> DeleteBanner(string id)
    {
        try
        {
            await _bannerService.DeleteAsync(id);
            return Ok(ApiResponse.CreateSuccess("Banner deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting banner");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeleteBannerFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-22: Update Premium Package Price
    /// </summary>
    [HttpPut("premium-packages/{id}/price")]
    public async Task<IActionResult> UpdatePackagePrice(string id, [FromBody] UpdatePriceRequest request)
    {
        try
        {
            // Convert VND to minor units
            var priceInMinorUnits = (long)(request.Price * 100);
            var succeeded = await _premiumPackageService.UpdatePriceAsync(id, priceInMinorUnits);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Package.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Package price updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating package price");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "UpdatePriceFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get all premium packages
    /// GET /admin/premium-packages
    /// </summary>
    [HttpGet("premium-packages")]
    public async Task<IActionResult> GetPremiumPackages()
    {
        try
        {
            var packages = await _premiumPackageService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<PremiumPackage>>.CreateSuccess(packages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium packages");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPackagesFailed"));
        }
    }

    /// <summary>
    /// UC-47: Get single premium package detail
    /// GET /admin/premium-packages/{id}
    /// </summary>
    [HttpGet("premium-packages/{id}")]
    public async Task<IActionResult> GetPremiumPackageById(string id)
    {
        try
        {
            var package = await _premiumPackageService.GetByIdAsync(id);
            if (package == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Package.NotFound), "NotFound"));

            return Ok(ApiResponse<PremiumPackage>.CreateSuccess(package));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting package: {PackageId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPackageFailed"));
        }
    }

    /// <summary>
    /// UC-47: Create premium package
    /// POST /admin/premium-packages
    /// </summary>
    [HttpPost("premium-packages")]
    public async Task<IActionResult> CreatePremiumPackage([FromBody] CreatePremiumPackageRequest request)
    {
        try
        {
            var package = new PremiumPackage
            {
                Name = request.Name,
                Description = request.Description,
                Price = (long)(request.Price * 100),
                DurationDays = request.DurationDays,
                IsActive = request.IsActive ?? true
            };
            var created = await _premiumPackageService.CreateAsync(package);
            return Ok(ApiResponse.CreateSuccess(created, "Package created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premium package");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "CreatePackageFailed"));
        }
    }

    /// <summary>
    /// UC-47: Delete premium package
    /// DELETE /admin/premium-packages/{id}
    /// </summary>
    [HttpDelete("premium-packages/{id}")]
    public async Task<IActionResult> DeletePremiumPackage(string id)
    {
        try
        {
            var succeeded = await _premiumPackageService.DeleteAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Package.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Package deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting premium package: {PackageId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "DeletePackageFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-23: View Payment Requests (fix stub — now queries DB)
    /// GET /admin/payments?status=
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] string? status = null)
    {
        try
        {
            var payments = await _adminService.GetAllPaymentsAsync(status);
            return Ok(ApiResponse<IEnumerable<Payment>>.CreateSuccess(payments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPaymentsFailed"));
        }
    }

    /// <summary>
    /// Payment Oversight: Get single payment detail
    /// GET /admin/payments/{id}
    /// </summary>
    [HttpGet("payments/{id}")]
    public async Task<IActionResult> GetPaymentById(string id)
    {
        try
        {
            var payment = await _adminService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.NotFound), "NotFound"));

            return Ok(ApiResponse<Payment>.CreateSuccess(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment: {PaymentId}", id);
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetPaymentFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-24: Approve/Reject Payment Request
    /// </summary>
    [HttpPut("payments/{id}/approve")]
    public async Task<IActionResult> ApprovePayment(string id)
    {
        try
        {
            var succeeded = await _adminService.ApprovePaymentRequestAsync(id);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Payment approved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payment");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "ApprovePaymentFailed"));
        }
    }

    [HttpPut("payments/{id}/reject")]
    public async Task<IActionResult> RejectPayment(string id, [FromBody] RejectPaymentRequest request)
    {
        try
        {
            var succeeded = await _adminService.RejectPaymentRequestAsync(id, request.Reason);
            if (!succeeded)
                return NotFound(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Payment.NotFound), "NotFound"));

            return Ok(ApiResponse.CreateSuccess("Payment rejected successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payment");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "RejectPaymentFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-25: View Audit Logs
    /// </summary>
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? userId = null, [FromQuery] string? entityType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var logs = await _adminService.GetAuditLogsAsync(userId, entityType, fromDate, toDate);
            return Ok(ApiResponse.CreateSuccess(logs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "GetAuditLogsFailed"));
        }
    }

    /// <summary>
    /// UC-48: Export Audit Logs as CSV
    /// GET /admin/audit-logs/export?format=csv
    /// </summary>
    [HttpGet("audit-logs/export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string format = "csv")
    {
        try
        {
            var csvBytes = await _adminService.ExportAuditLogsCsvAsync(userId, entityType, fromDate, toDate);
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
            return File(csvBytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "ExportAuditLogsFailed"));
        }
    }

    /// <summary>
    /// UC-ADM-26: Send Broadcast Notification
    /// </summary>
    [HttpPost("notifications/broadcast")]
    public async Task<IActionResult> SendBroadcast([FromBody] BroadcastNotificationRequest request)
    {
        try
        {
            var succeeded = await _adminService.SendBroadcastNotificationAsync(request.Title, request.Message, request.TargetRole);
            if (!succeeded)
                return BadRequest(ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.BadRequest), "BroadcastFailed"));

            return Ok(ApiResponse.CreateSuccess("Broadcast notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending broadcast notification");
            return StatusCode(500, ApiResponse.CreateError(_i18nService.GetErrorMessage(I18nKeys.Error.Common.InternalServerError), "BroadcastFailed"));
        }
    }
}

public record RejectVerificationRequest(string Reason);
public record HideJobRequest(string Reason);
public record CreateCategoryRequest(string Name);
public record UpdateCategoryRequest(string Name);
public record CreateSkillRequest(string Name, string? CategoryId = null);
public record UpdateSkillRequest(string Name, string? CategoryId = null);
public record CreateBannerRequest(string Title, string ImageUrl, string? LinkUrl = null, int Order = 0, bool? IsActive = true);
public record UpdateBannerRequest(string? Title = null, string? ImageUrl = null, string? LinkUrl = null, int? Order = null, bool? IsActive = null);
public record UpdatePriceRequest(decimal Price);
public record RejectPaymentRequest(string Reason);
public record BroadcastNotificationRequest(string Title, string Message, string? TargetRole = null);
public record CreatePremiumPackageRequest(
    string Name,
    string? Description = null,
    decimal Price = 0,
    int DurationDays = 30,
    bool? IsActive = true);


