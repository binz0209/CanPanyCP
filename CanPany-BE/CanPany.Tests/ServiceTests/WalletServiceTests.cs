using CanPany.Application.Services;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepositoryMock = new();
    private readonly Mock<ILogger<WalletService>> _loggerMock = new();
    private readonly WalletService _service;

    public WalletServiceTests()
    {
        _service = new WalletService(_walletRepositoryMock.Object, _transactionRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnWallet_WhenExists()
    {
        // Arrange
        var userId = "user123";
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 50000000L
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(wallet.Id, result.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var userId = "user123";
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnBalance_WhenWalletExists()
    {
        // Arrange
        var userId = "user123";
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 100000000L
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.GetBalanceAsync(userId);

        // Assert
        Assert.Equal(100000000L, result);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldCreateWallet_WhenNotExists()
    {
        // Arrange
        var userId = "user123";
        var newWallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 0
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet?)null);
        _walletRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Wallet>()))
            .ReturnsAsync(newWallet);

        // Act
        var result = await _service.GetBalanceAsync(userId);

        // Assert
        Assert.Equal(0L, result);
    }

    [Fact]
    public async Task EnsureAsync_ShouldReturnExistingWallet_WhenExists()
    {
        // Arrange
        var userId = "user123";
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 50000000L
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.EnsureAsync(userId);

        // Assert
        Assert.Equal(wallet, result);
    }

    [Fact]
    public async Task EnsureAsync_ShouldCreateNewWallet_WhenNotExists()
    {
        // Arrange
        var userId = "user123";
        var newWallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 0
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Wallet?)null);
        _walletRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Wallet>()))
            .ReturnsAsync(newWallet);

        // Act
        var result = await _service.EnsureAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task ChangeBalanceAsync_ShouldReturnSuccess_WhenSufficientBalance()
    {
        // Arrange
        var userId = "user123";
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 100000000L
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);
        _walletRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Wallet>()))
            .Returns(Task.CompletedTask);
        _transactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<WalletTransaction>()))
            .ReturnsAsync(new WalletTransaction { Id = "trans1" });

        // Act
        var result = await _service.ChangeBalanceAsync(userId, -50000000L, "Withdrawal");

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ChangeBalanceAsync_ShouldReturnError_WhenInsufficientBalance()
    {
        // Arrange
        var userId = "user123";
        var wallet = new Wallet
        {
            Id = "wallet123",
            UserId = userId,
            Balance = 10000000L // 100,000 VND
        };
        
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _service.ChangeBalanceAsync(userId, -50000000L, "Withdrawal"); // Try to withdraw 500,000 VND

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_ShouldReturnTransactions_WhenExist()
    {
        // Arrange
        var userId = "user123";
        var transactions = new List<WalletTransaction>
        {
            new WalletTransaction { Id = "trans1", UserId = userId, Amount = 10000000L },
            new WalletTransaction { Id = "trans2", UserId = userId, Amount = -5000000L }
        };
        
        _transactionRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionHistoryAsync(userId, 20);

        // Assert
        Assert.Equal(2, result.Count());
    }
}
