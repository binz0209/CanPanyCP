using CanPany.Application.DTOs;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _reportRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IAdminService> _adminServiceMock = new();
    private readonly Mock<ILogger<ReportService>> _loggerMock = new();
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _service = new ReportService(
            _reportRepoMock.Object,
            _userRepoMock.Object,
            _notificationServiceMock.Object,
            _adminServiceMock.Object,
            _loggerMock.Object);
    }

    // ==================== CreateReportAsync ====================

    [Fact]
    public async Task CreateReportAsync_ShouldCreateReport_WhenValid()
    {
        // Arrange
        var reporterId = "reporter1";
        var dto = new CreateReportDto("reported1", "Spam", "Spamming messages");
        var reportedUser = new User { Id = "reported1", FullName = "Reported User", Email = "reported@test.com" };
        var reporter = new User { Id = "reporter1", FullName = "Reporter", Email = "reporter@test.com" };

        _userRepoMock.Setup(x => x.GetByIdAsync("reported1")).ReturnsAsync(reportedUser);
        _userRepoMock.Setup(x => x.GetByIdAsync("reporter1")).ReturnsAsync(reporter);
        _userRepoMock.Setup(x => x.GetByRoleAsync("Admin")).ReturnsAsync(new List<User>());
        _reportRepoMock.Setup(x => x.AddAsync(It.IsAny<Report>()))
            .ReturnsAsync((Report r) => { r.Id = "report1"; return r; });

        // Act
        var result = await _service.CreateReportAsync(reporterId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Spam", result.Reason);
        Assert.Equal("Spamming messages", result.Description);
        _reportRepoMock.Verify(x => x.AddAsync(It.IsAny<Report>()), Times.Once);
    }

    [Fact]
    public async Task CreateReportAsync_ShouldThrow_WhenReportedUserNotFound()
    {
        // Arrange
        var dto = new CreateReportDto("nonexistent", "Spam", "Spamming");
        _userRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReportAsync("reporter1", dto));
    }

    // ==================== GetReportByIdAsync ====================

    [Fact]
    public async Task GetReportByIdAsync_ShouldReturnReport_WhenExists()
    {
        // Arrange
        var report = new Report
        {
            Id = "r1",
            ReporterId = "reporter1",
            ReportedUserId = "reported1",
            Reason = "Spam",
            Description = "Spamming",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        var reporter = new User { Id = "reporter1", FullName = "Reporter", Email = "r@test.com" };
        var reportedUser = new User { Id = "reported1", FullName = "Reported", Email = "rd@test.com" };

        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);
        _userRepoMock.Setup(x => x.GetByIdAsync("reporter1")).ReturnsAsync(reporter);
        _userRepoMock.Setup(x => x.GetByIdAsync("reported1")).ReturnsAsync(reportedUser);

        // Act
        var result = await _service.GetReportByIdAsync("r1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("r1", result.Id);
        Assert.Equal("Spam", result.Reason);
    }

    [Fact]
    public async Task GetReportByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _reportRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Report?)null);

        // Act
        var result = await _service.GetReportByIdAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    // ==================== GetReportsByReporterIdAsync ====================

    [Fact]
    public async Task GetReportsByReporterIdAsync_ShouldReturnReports()
    {
        // Arrange
        var reports = new List<Report>
        {
            new() { Id = "r1", ReporterId = "reporter1", ReportedUserId = "u1", Reason = "Spam", Description = "Spam", Status = "Pending", CreatedAt = DateTime.UtcNow },
            new() { Id = "r2", ReporterId = "reporter1", ReportedUserId = "u2", Reason = "Fraud", Description = "Fraud", Status = "Pending", CreatedAt = DateTime.UtcNow }
        };
        var reporter = new User { Id = "reporter1", FullName = "Reporter", Email = "r@test.com" };
        var u1 = new User { Id = "u1", FullName = "User1", Email = "u1@test.com" };
        var u2 = new User { Id = "u2", FullName = "User2", Email = "u2@test.com" };

        _reportRepoMock.Setup(x => x.GetByReporterIdAsync("reporter1")).ReturnsAsync(reports);
        _userRepoMock.Setup(x => x.GetByIdAsync("reporter1")).ReturnsAsync(reporter);
        _userRepoMock.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(u1);
        _userRepoMock.Setup(x => x.GetByIdAsync("u2")).ReturnsAsync(u2);

        // Act
        var result = (await _service.GetReportsByReporterIdAsync("reporter1")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    // ==================== GetAllReportsAsync ====================

    [Fact]
    public async Task GetAllReportsAsync_ShouldReturnAllReports_WhenNoFilter()
    {
        // Arrange
        var reports = new List<Report>
        {
            new() { Id = "r1", ReporterId = "reporter1", ReportedUserId = "u1", Reason = "Spam", Description = "d", Status = "Pending", CreatedAt = DateTime.UtcNow }
        };
        var reporter = new User { Id = "reporter1", FullName = "Reporter", Email = "r@test.com" };
        var u1 = new User { Id = "u1", FullName = "User1", Email = "u1@test.com" };

        _reportRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(reports);
        _userRepoMock.Setup(x => x.GetByIdAsync("reporter1")).ReturnsAsync(reporter);
        _userRepoMock.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(u1);

        // Act
        var result = (await _service.GetAllReportsAsync()).ToList();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllReportsAsync_ShouldApplyFilter_WhenFilterProvided()
    {
        // Arrange
        var filter = new ReportFilterDto(Status: "Pending");
        var reports = new List<Report>
        {
            new() { Id = "r1", ReporterId = "reporter1", ReportedUserId = "u1", Reason = "Spam", Description = "d", Status = "Pending", CreatedAt = DateTime.UtcNow }
        };
        var reporter = new User { Id = "reporter1", FullName = "Reporter", Email = "r@test.com" };
        var u1 = new User { Id = "u1", FullName = "User1", Email = "u1@test.com" };

        _reportRepoMock.Setup(x => x.GetWithFiltersAsync("Pending", null, null)).ReturnsAsync(reports);
        _userRepoMock.Setup(x => x.GetByIdAsync("reporter1")).ReturnsAsync(reporter);
        _userRepoMock.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(u1);

        // Act
        var result = (await _service.GetAllReportsAsync(filter)).ToList();

        // Assert
        Assert.Single(result);
        _reportRepoMock.Verify(x => x.GetWithFiltersAsync("Pending", null, null), Times.Once);
    }

    // ==================== ResolveReportAsync ====================

    [Fact]
    public async Task ResolveReportAsync_ShouldResolve_WhenPending()
    {
        // Arrange
        var report = new Report
        {
            Id = "r1",
            ReporterId = "reporter1",
            ReportedUserId = "reported1",
            Status = "Pending",
            Reason = "Spam",
            Description = "d"
        };

        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);
        _reportRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(new Notification());

        // Act
        var result = await _service.ResolveReportAsync("r1", "admin1", "Resolved", false);

        // Assert
        Assert.True(result);
        Assert.Equal("Resolved", report.Status);
        _reportRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Report>()), Times.Once);
    }

    [Fact]
    public async Task ResolveReportAsync_ShouldReturnFalse_WhenReportNotFound()
    {
        // Arrange
        _reportRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Report?)null);

        // Act
        var result = await _service.ResolveReportAsync("nonexistent", "admin1", "Note", false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResolveReportAsync_ShouldReturnFalse_WhenNotPending()
    {
        // Arrange
        var report = new Report { Id = "r1", Status = "Resolved", ReporterId = "r", Reason = "x", Description = "d" };
        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);

        // Act
        var result = await _service.ResolveReportAsync("r1", "admin1", "Note", false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResolveReportAsync_ShouldBanUser_WhenBanUserTrue()
    {
        // Arrange
        var report = new Report
        {
            Id = "r1",
            ReporterId = "reporter1",
            ReportedUserId = "reported1",
            Status = "Pending",
            Reason = "Spam",
            Description = "d"
        };

        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);
        _reportRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
        _adminServiceMock.Setup(x => x.BanUserAsync("reported1")).ReturnsAsync(true);
        _notificationServiceMock.Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(new Notification());

        // Act
        var result = await _service.ResolveReportAsync("r1", "admin1", "Banned for spam", true);

        // Assert
        Assert.True(result);
        _adminServiceMock.Verify(x => x.BanUserAsync("reported1"), Times.Once);
    }

    // ==================== RejectReportAsync ====================

    [Fact]
    public async Task RejectReportAsync_ShouldReject_WhenPending()
    {
        // Arrange
        var report = new Report
        {
            Id = "r1",
            ReporterId = "reporter1",
            Status = "Pending",
            Reason = "Spam",
            Description = "d"
        };

        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);
        _reportRepoMock.Setup(x => x.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(x => x.CreateAsync(It.IsAny<Notification>()))
            .ReturnsAsync(new Notification());

        // Act
        var result = await _service.RejectReportAsync("r1", "admin1", "Not valid");

        // Assert
        Assert.True(result);
        Assert.Equal("Rejected", report.Status);
    }

    [Fact]
    public async Task RejectReportAsync_ShouldReturnFalse_WhenNotPending()
    {
        // Arrange
        var report = new Report { Id = "r1", Status = "Resolved", ReporterId = "r", Reason = "x", Description = "d" };
        _reportRepoMock.Setup(x => x.GetByIdAsync("r1")).ReturnsAsync(report);

        // Act
        var result = await _service.RejectReportAsync("r1", "admin1", "Reason");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RejectReportAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        _reportRepoMock.Setup(x => x.GetByIdAsync("nonexistent")).ReturnsAsync((Report?)null);

        // Act
        var result = await _service.RejectReportAsync("nonexistent", "admin1", "Reason");

        // Assert
        Assert.False(result);
    }
}
