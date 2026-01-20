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

public class WalletControllerTests
{
    private readonly Mock<IWalletService> _walletServiceMock = new();
    private readonly Mock<ILogger<WalletController>> _loggerMock = new();
    private readonly WalletController _controller;

    public WalletControllerTests()
    {
        _controller = new WalletController(
            _walletServiceMock.Object,
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
    public async Task GetBalance_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var userId = "user123";
        var balance = 50000000L; // 500,000 VND in minor units
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = balance
        };
        
        _walletServiceMock.Setup(x => x.GetBalanceAsync(userId))
            .ReturnsAsync(balance);
        _walletServiceMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _controller.GetBalance();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetBalance_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var result = await _controller.GetBalance();

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnSuccess_WhenTransactionsExist()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<WalletTransaction>
        {
            new WalletTransaction 
            { 
                Id = "trans1", 
                WalletId = "wallet123", 
                Amount = 10000000,
                Type = "TopUp",
                CreatedAt = DateTime.UtcNow
            },
            new WalletTransaction 
            { 
                Id = "trans2", 
                WalletId = "wallet123", 
                Amount = 5000000,
                Type = "Withdraw",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };
        
        _walletServiceMock.Setup(x => x.GetTransactionHistoryAsync(userId, 20))
            .ReturnsAsync(transactions);

        // Act
        var result = await _controller.GetTransactions(20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<WalletTransaction>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data?.Count());
    }

    [Fact]
    public async Task GetTransactions_ShouldUseDefaultTake_WhenNotProvided()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<WalletTransaction>();
        
        _walletServiceMock.Setup(x => x.GetTransactionHistoryAsync(userId, 20))
            .ReturnsAsync(transactions);

        // Act
        var result = await _controller.GetTransactions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<WalletTransaction>>>(okResult.Value);
        Assert.True(response.Success);
    }
}
