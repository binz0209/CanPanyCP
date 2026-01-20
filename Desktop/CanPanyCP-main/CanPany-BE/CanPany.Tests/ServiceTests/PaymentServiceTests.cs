using CanPany.Application.Services;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly Mock<IWalletService> _walletServiceMock = new();
    private readonly Mock<ILogger<PaymentService>> _loggerMock = new();
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _service = new PaymentService(_repositoryMock.Object, _walletServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var paymentId = "payment123";
        var payment = new Payment
        {
            Id = paymentId,
            UserId = "user123",
            Amount = 100000000,
            Status = "Pending"
        };
        
        _repositoryMock.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _service.GetByIdAsync(paymentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(paymentId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(""));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnPayments_WhenExist()
    {
        // Arrange
        var userId = "user123";
        var payments = new List<Payment>
        {
            new Payment { Id = "payment1", UserId = userId, Amount = 100000000 },
            new Payment { Id = "payment2", UserId = userId, Amount = 50000000 }
        };
        
        _repositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(payments);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateDepositRequestAsync_ShouldReturnPayment_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var amount = 100000000L; // 1,000,000 VND in minor units
        var payment = new Payment
        {
            Id = "payment123",
            UserId = userId,
            Amount = amount,
            Purpose = "TopUp",
            Status = "Pending"
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _service.CreateDepositRequestAsync(userId, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TopUp", result.Purpose);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task CreateDepositRequestAsync_ShouldThrow_WhenAmountIsZero()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateDepositRequestAsync("user123", 0));
    }

    [Fact]
    public async Task CreatePremiumPurchaseAsync_ShouldReturnPayment_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var packageId = "package123";
        var amount = 50000000L; // 500,000 VND in minor units
        var payment = new Payment
        {
            Id = "payment123",
            UserId = userId,
            Amount = amount,
            Purpose = "PremiumPurchase",
            Status = "Pending"
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _service.CreatePremiumPurchaseAsync(userId, packageId, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PremiumPurchase", result.Purpose);
    }

    [Fact]
    public async Task CreatePremiumPurchaseAsync_ShouldThrow_WhenPackageIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreatePremiumPurchaseAsync("user123", "", 100000000));
    }
}
