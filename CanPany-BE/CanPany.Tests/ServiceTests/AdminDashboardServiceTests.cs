using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using JobApplication = CanPany.Domain.Entities.Application;

namespace CanPany.Tests.ServiceTests;

public class AdminDashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<IApplicationRepository> _applicationRepoMock = new();
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<ILogger<AdminDashboardService>> _loggerMock = new();
    private readonly AdminDashboardService _service;

    public AdminDashboardServiceTests()
    {
        _service = new AdminDashboardService(
            _userRepoMock.Object,
            _jobRepoMock.Object,
            _applicationRepoMock.Object,
            _companyRepoMock.Object,
            _paymentRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        var users = new List<User> { new() { Id = "u1" }, new() { Id = "u2" } };
        var jobs = new List<Job> { new() { Id = "j1" } };
        var applications = new List<JobApplication> { new() { Id = "a1" }, new() { Id = "a2" }, new() { Id = "a3" } };
        var companies = new List<Company>
        {
            new() { Id = "co1", VerificationStatus = "Pending" },
            new() { Id = "co2", VerificationStatus = "Verified" }
        };
        var pendingPayments = new List<Payment> { new() { Id = "p1", Amount = 50000 } };
        var paidPayments = new List<Payment>
        {
            new() { Id = "p2", Amount = 10000000 },
            new() { Id = "p3", Amount = 5000000 }
        };

        _userRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
        _jobRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(jobs);
        _applicationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(applications);
        _companyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(companies);
        _paymentRepoMock.Setup(x => x.GetByStatusAsync("Pending")).ReturnsAsync(pendingPayments);
        _paymentRepoMock.Setup(x => x.GetByStatusAsync("Paid")).ReturnsAsync(paidPayments);

        // Act
        var result = await _service.GetDashboardStatsAsync();

        // Assert
        Assert.Equal(2, result.TotalUsers);
        Assert.Equal(1, result.TotalJobs);
        Assert.Equal(3, result.TotalApplications);
        Assert.Equal(2, result.TotalCompanies);
        Assert.Equal(1, result.PendingVerifications);
        Assert.Equal(1, result.PendingPayments);
        Assert.Equal(150000m, result.TotalRevenue);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldHandleEmptyData()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User>());
        _jobRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Job>());
        _applicationRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<JobApplication>());
        _companyRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Company>());
        _paymentRepoMock.Setup(x => x.GetByStatusAsync("Pending")).ReturnsAsync(new List<Payment>());
        _paymentRepoMock.Setup(x => x.GetByStatusAsync("Paid")).ReturnsAsync(new List<Payment>());

        // Act
        var result = await _service.GetDashboardStatsAsync();

        // Assert
        Assert.Equal(0, result.TotalUsers);
        Assert.Equal(0, result.TotalJobs);
        Assert.Equal(0, result.TotalApplications);
        Assert.Equal(0, result.TotalCompanies);
        Assert.Equal(0, result.PendingVerifications);
        Assert.Equal(0, result.PendingPayments);
        Assert.Equal(0m, result.TotalRevenue);
    }
}
