using CanPany.Application.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class AdminServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<ICompanyRepository> _companyRepositoryMock = new();
    private readonly Mock<IJobRepository> _jobRepositoryMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<INotificationRepository> _notificationRepositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly Mock<ILogger<AdminService>> _loggerMock = new();
    private readonly AdminService _service;

    public AdminServiceTests()
    {
        _service = new AdminService(
            _userRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _jobRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task BanUserAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            IsLocked = false
        };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.BanUserAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task BanUserAsync_ShouldReturnFalse_WhenUserNotExists()
    {
        // Arrange
        var userId = "nonexistent";
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.BanUserAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UnbanUserAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            IsLocked = true
        };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UnbanUserAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ApproveCompanyVerificationAsync_ShouldReturnTrue_WhenCompanyExists()
    {
        // Arrange
        var companyId = "company123";
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsVerified = false
        };
        
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);
        _companyRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Company>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ApproveCompanyVerificationAsync(companyId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RejectCompanyVerificationAsync_ShouldReturnTrue_WhenCompanyExists()
    {
        // Arrange
        var companyId = "company123";
        var reason = "Invalid documents";
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            IsVerified = false
        };
        
        _companyRepositoryMock.Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);
        _companyRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Company>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RejectCompanyVerificationAsync(companyId, reason);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HideJobAsync_ShouldReturnTrue_WhenJobExists()
    {
        // Arrange
        var jobId = "job123";
        var reason = "Violation of terms";
        var job = new Job
        {
            Id = jobId,
            Title = "Test Job",
            Status = "Active"
        };
        
        _jobRepositoryMock.Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync(job);
        _jobRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Job>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.HideJobAsync(jobId, reason);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteJobAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var jobId = "job123";
        _jobRepositoryMock.Setup(x => x.DeleteAsync(jobId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteJobAsync(jobId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ApprovePaymentRequestAsync_ShouldReturnTrue_WhenPaymentExists()
    {
        // Arrange
        var paymentId = "payment123";
        var payment = new Payment
        {
            Id = paymentId,
            Status = "Pending"
        };
        
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);
        _paymentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ApprovePaymentRequestAsync(paymentId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RejectPaymentRequestAsync_ShouldReturnTrue_WhenPaymentExists()
    {
        // Arrange
        var paymentId = "payment123";
        var reason = "Invalid payment";
        var payment = new Payment
        {
            Id = paymentId,
            Status = "Pending"
        };
        
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);
        _paymentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RejectPaymentRequestAsync(paymentId, reason);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnLogs_WhenValid()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = "log1", UserId = "user1", Action = "Create" },
            new AuditLog { Id = "log2", UserId = "user2", Action = "Update" }
        };
        
        _auditLogRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetAuditLogsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SendBroadcastNotificationAsync_ShouldReturnTrue_WhenValid()
    {
        // Arrange
        var title = "System Maintenance";
        var message = "The system will be under maintenance";
        var targetRole = "Candidate";

        // Act
        var result = await _service.SendBroadcastNotificationAsync(title, message, targetRole);

        // Assert
        Assert.True(result);
    }
}
