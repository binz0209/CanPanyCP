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

namespace CanPany.Tests.ControllerTests;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly Mock<ILogger<PaymentsController>> _loggerMock = new();
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _controller = new PaymentsController(
            _paymentServiceMock.Object,
            _loggerMock.Object);
        
        // Setup authenticated user
        var claims = new List<Claim> { new Claim("sub", "user123") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CreateDeposit_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var request = new CreateDepositRequest(1000000); // 1,000,000 VND
        var payment = new Payment
        {
            Id = "payment123",
            UserId = userId,
            Amount = 100000000, // in minor units (100 * 1000000)
            Purpose = "TopUp",
            Status = "Pending"
        };
        
        _paymentServiceMock.Setup(x => x.CreateDepositRequestAsync(userId, It.IsAny<long>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.CreateDeposit(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Payment>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateDeposit_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };
        var request = new CreateDepositRequest(1000000);

        // Act
        var result = await _controller.CreateDeposit(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task PurchasePremium_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var request = new PurchasePremiumRequest("package123", 500000); // 500,000 VND
        var payment = new Payment
        {
            Id = "payment123",
            UserId = userId,
            Amount = 50000000, // in minor units
            Purpose = "PremiumPurchase",
            Status = "Pending"
        };
        
        _paymentServiceMock.Setup(x => x.CreatePremiumPurchaseAsync(userId, request.PackageId, It.IsAny<long>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _controller.PurchasePremium(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Payment>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetPayments_ShouldReturnSuccess_WhenPaymentsExist()
    {
        // Arrange
        var userId = "user123";
        var payments = new List<Payment>
        {
            new Payment { Id = "payment1", UserId = userId, Amount = 100000000, Status = "Paid" },
            new Payment { Id = "payment2", UserId = userId, Amount = 50000000, Status = "Pending" }
        };
        
        _paymentServiceMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(payments);

        // Act
        var result = await _controller.GetPayments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<Payment>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task PaymentCallback_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var paymentData = new Dictionary<string, string>
        {
            { "vnp_TxnRef", "payment123" },
            { "vnp_ResponseCode", "00" }
        };
        
        _paymentServiceMock.Setup(x => x.ProcessPaymentAsync("payment123", paymentData))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PaymentCallback(paymentData);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task PaymentCallback_ShouldReturnBadRequest_WhenInvalidData()
    {
        // Arrange
        var paymentData = new Dictionary<string, string>();

        // Act
        var result = await _controller.PaymentCallback(paymentData);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}
