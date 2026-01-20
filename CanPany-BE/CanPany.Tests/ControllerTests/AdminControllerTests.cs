using CanPany.Api.Controllers;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Models;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using DashboardStatsDto = CanPany.Application.Interfaces.Services.DashboardStatsDto;

namespace CanPany.Tests.ControllerTests;

public class AdminControllerTests
{
    private readonly Mock<IAdminDashboardService> _dashboardServiceMock = new();
    private readonly Mock<IAdminService> _adminServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ICompanyService> _companyServiceMock = new();
    private readonly Mock<IJobService> _jobServiceMock = new();
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<ISkillService> _skillServiceMock = new();
    private readonly Mock<IBannerService> _bannerServiceMock = new();
    private readonly Mock<IPremiumPackageService> _premiumPackageServiceMock = new();
    private readonly Mock<IReportService> _reportServiceMock = new();
    private readonly Mock<ILogger<AdminController>> _loggerMock = new();
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _controller = new AdminController(
            _dashboardServiceMock.Object,
            _adminServiceMock.Object,
            _userServiceMock.Object,
            _companyServiceMock.Object,
            _jobServiceMock.Object,
            _paymentServiceMock.Object,
            _categoryServiceMock.Object,
            _skillServiceMock.Object,
            _bannerServiceMock.Object,
            _premiumPackageServiceMock.Object,
            _reportServiceMock.Object,
            _loggerMock.Object);
        
        // Setup authenticated admin user
        var claims = new List<Claim> 
        { 
            new Claim("sub", "admin123"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetDashboard_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var stats = new DashboardStatsDto { TotalUsers = 100, TotalJobs = 50, TotalCompanies = 20 };
        _dashboardServiceMock.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetDashboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnSuccess_WhenUsersExist()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = "user1", Email = "user1@example.com" },
            new User { Id = "user2", Email = "user2@example.com" }
        };
        _userServiceMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<User>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task BanUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        _adminServiceMock.Setup(x => x.BanUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.BanUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task BanUser_ShouldReturnNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = "nonexistent";
        _adminServiceMock.Setup(x => x.BanUserAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.BanUser(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UnbanUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        _adminServiceMock.Setup(x => x.UnbanUserAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UnbanUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ApproveVerification_ShouldReturnSuccess_WhenCompanyExists()
    {
        // Arrange
        var companyId = "company123";
        _adminServiceMock.Setup(x => x.ApproveCompanyVerificationAsync(companyId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApproveVerification(companyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task RejectVerification_ShouldReturnSuccess_WhenCompanyExists()
    {
        // Arrange
        var companyId = "company123";
        var request = new RejectVerificationRequest("Invalid documents");
        _adminServiceMock.Setup(x => x.RejectCompanyVerificationAsync(companyId, request.Reason))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectVerification(companyId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task HideJob_ShouldReturnSuccess_WhenJobExists()
    {
        // Arrange
        var jobId = "job123";
        var request = new HideJobRequest("Violation of terms");
        _adminServiceMock.Setup(x => x.HideJobAsync(jobId, request.Reason))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.HideJob(jobId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteJob_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var jobId = "job123";
        _adminServiceMock.Setup(x => x.DeleteJobAsync(jobId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteJob(jobId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new CreateCategoryRequest("Technology");
        var category = new Category { Id = "cat1", Name = request.Name };
        _categoryServiceMock.Setup(x => x.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Category>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnSuccess_WhenCategoryExists()
    {
        // Arrange
        var categoryId = "cat1";
        var request = new UpdateCategoryRequest("Updated Technology");
        var category = new Category { Id = categoryId, Name = "Technology" };
        _categoryServiceMock.Setup(x => x.GetByIdAsync(categoryId))
            .ReturnsAsync(category);
        _categoryServiceMock.Setup(x => x.UpdateAsync(categoryId, It.IsAny<Category>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCategory(categoryId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var categoryId = "cat1";
        _categoryServiceMock.Setup(x => x.DeleteAsync(categoryId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCategory(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateSkill_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new CreateSkillRequest("C#", "cat1");
        var skill = new Skill { Id = "skill1", Name = request.Name, CategoryId = request.CategoryId };
        _skillServiceMock.Setup(x => x.CreateAsync(It.IsAny<Skill>()))
            .ReturnsAsync(skill);

        // Act
        var result = await _controller.CreateSkill(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Skill>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateSkill_ShouldReturnSuccess_WhenSkillExists()
    {
        // Arrange
        var skillId = "skill1";
        var request = new UpdateSkillRequest("C# .NET", "cat1");
        var skill = new Skill { Id = skillId, Name = "C#", CategoryId = "cat1" };
        _skillServiceMock.Setup(x => x.GetByIdAsync(skillId))
            .ReturnsAsync(skill);
        _skillServiceMock.Setup(x => x.UpdateAsync(skillId, It.IsAny<Skill>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateSkill(skillId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteSkill_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var skillId = "skill1";
        _skillServiceMock.Setup(x => x.DeleteAsync(skillId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSkill(skillId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateBanner_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new CreateBannerRequest("Banner Title", "https://example.com/image.jpg", "https://example.com", 1, true);
        var banner = new Banner 
        { 
            Id = "banner1", 
            Title = request.Title, 
            ImageUrl = request.ImageUrl,
            LinkUrl = request.LinkUrl,
            Order = request.Order,
            IsActive = request.IsActive ?? true
        };
        _bannerServiceMock.Setup(x => x.CreateAsync(It.IsAny<Banner>()))
            .ReturnsAsync(banner);

        // Act
        var result = await _controller.CreateBanner(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Banner>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateBanner_ShouldReturnSuccess_WhenBannerExists()
    {
        // Arrange
        var bannerId = "banner1";
        var request = new UpdateBannerRequest("Updated Title", null, null, null, null);
        var banner = new Banner { Id = bannerId, Title = "Original Title" };
        _bannerServiceMock.Setup(x => x.GetByIdAsync(bannerId))
            .ReturnsAsync(banner);
        _bannerServiceMock.Setup(x => x.UpdateAsync(bannerId, It.IsAny<Banner>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateBanner(bannerId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteBanner_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var bannerId = "banner1";
        _bannerServiceMock.Setup(x => x.DeleteAsync(bannerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteBanner(bannerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ApprovePayment_ShouldReturnSuccess_WhenPaymentExists()
    {
        // Arrange
        var paymentId = "payment123";
        _adminServiceMock.Setup(x => x.ApprovePaymentRequestAsync(paymentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ApprovePayment(paymentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task RejectPayment_ShouldReturnSuccess_WhenPaymentExists()
    {
        // Arrange
        var paymentId = "payment123";
        var request = new RejectPaymentRequest("Invalid payment");
        _adminServiceMock.Setup(x => x.RejectPaymentRequestAsync(paymentId, request.Reason))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.RejectPayment(paymentId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = "log1", UserId = "user1", Action = "Create" },
            new AuditLog { Id = "log2", UserId = "user2", Action = "Update" }
        };
        _adminServiceMock.Setup(x => x.GetAuditLogsAsync(null, null, null, null))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetAuditLogs(null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<AuditLog>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task SendBroadcast_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new BroadcastNotificationRequest("Title", "Message", "Candidate");
        _adminServiceMock.Setup(x => x.SendBroadcastNotificationAsync(request.Title, request.Message, request.TargetRole))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendBroadcast(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
