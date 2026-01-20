using CanPany.Application.Services;
using CanPany.Domain.Entities;
using CanPany.Domain.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IHashService> _hashServiceMock = new();
    private readonly Mock<IWalletService> _walletServiceMock = new();
    private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
    private readonly Mock<ICompanyService> _companyServiceMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(
            _userRepositoryMock.Object,
            _hashServiceMock.Object,
            _walletServiceMock.Object,
            _userProfileServiceMock.Object,
            _companyServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "user123";
        var user = new User { Id = userId, Email = "test@example.com", FullName = "Test User" };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotExists()
    {
        // Arrange
        var userId = "nonexistent";
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User { Id = "user123", Email = email, FullName = "Test User" };
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnUser_WhenValid()
    {
        // Arrange
        var fullName = "Test User";
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = "hashed-password";
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);
        _hashServiceMock.Setup(x => x.HashPassword(password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _walletServiceMock.Setup(x => x.EnsureAsync(It.IsAny<string>()))
            .ReturnsAsync(new Wallet { Id = "wallet123", UserId = "user123" });
        _userProfileServiceMock.Setup(x => x.GetByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync((UserProfile?)null);
        _userProfileServiceMock.Setup(x => x.CreateAsync(It.IsAny<UserProfile>()))
            .ReturnsAsync(new UserProfile { Id = "profile123", UserId = "user123" });

        // Act
        var result = await _userService.RegisterAsync(fullName, email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
        Assert.Equal(fullName, result.FullName);
        Assert.Equal(hashedPassword, result.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailExists()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User { Id = "user123", Email = email };
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _userService.RegisterAsync("Test User", email, "Password123!"));
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnUser_WhenValidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = new User 
        { 
            Id = "user123", 
            Email = email, 
            PasswordHash = "hashed-password" 
        };
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _hashServiceMock.Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _userService.ValidateUserAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenInvalidPassword()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var user = new User 
        { 
            Id = "user123", 
            Email = email, 
            PasswordHash = "hashed-password" 
        };
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);
        _hashServiceMock.Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _userService.ValidateUserAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnSuccess_WhenValidOldPassword()
    {
        // Arrange
        var userId = "user123";
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var user = new User 
        { 
            Id = userId, 
            Email = "test@example.com",
            PasswordHash = "old-hashed-password" 
        };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _hashServiceMock.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash))
            .Returns(true);
        _hashServiceMock.Setup(x => x.HashPassword(newPassword))
            .Returns("new-hashed-password");
        _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var (succeeded, errors) = await _userService.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.True(succeeded);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldReturnFailure_WhenInvalidOldPassword()
    {
        // Arrange
        var userId = "user123";
        var oldPassword = "WrongPassword";
        var newPassword = "NewPassword123!";
        var user = new User 
        { 
            Id = userId, 
            Email = "test@example.com",
            PasswordHash = "old-hashed-password" 
        };
        
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _hashServiceMock.Setup(x => x.VerifyPassword(oldPassword, user.PasswordHash))
            .Returns(false);

        // Act
        var (succeeded, errors) = await _userService.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.False(succeeded);
        Assert.NotEmpty(errors);
    }
}
