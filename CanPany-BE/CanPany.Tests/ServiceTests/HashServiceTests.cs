using CanPany.Infrastructure.Security.Hashing;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class HashServiceTests
{
    private readonly HashService _service = new();

    // ==================== BCrypt Tests ====================

    [Fact]
    public void HashPassword_ShouldReturnHash()
    {
        // Act
        var hash = _service.HashPassword("Password123!");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual("Password123!", hash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenCorrectPassword()
    {
        // Arrange
        var password = "MySecurePassword!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenWrongPassword()
    {
        // Arrange
        var hash = _service.HashPassword("CorrectPassword");

        // Act
        var result = _service.VerifyPassword("WrongPassword", hash);

        // Assert
        Assert.False(result);
    }

    // ==================== SHA-256 Tests ====================

    [Fact]
    public void ComputeSHA256_ShouldReturnConsistentHash()
    {
        // Arrange
        var input = "Hello, World!";

        // Act
        var hash1 = _service.ComputeSHA256(input);
        var hash2 = _service.ComputeSHA256(input);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // SHA-256 produces 32 bytes = 64 hex chars
    }

    [Fact]
    public void ComputeSHA256_DifferentInputs_ShouldProduceDifferentHashes()
    {
        // Act
        var hash1 = _service.ComputeSHA256("input1");
        var hash2 = _service.ComputeSHA256("input2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeSHA256_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Act
        var result = _service.ComputeSHA256("");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComputeSHA256_ShouldReturnEmpty_WhenInputIsNull()
    {
        // Act
        var result = _service.ComputeSHA256(null!);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComputeSHA256Bytes_ShouldReturnHash()
    {
        // Arrange
        var input = System.Text.Encoding.UTF8.GetBytes("Test data");

        // Act
        var hash = _service.ComputeSHA256Bytes(input);

        // Assert
        Assert.Equal(32, hash.Length); // SHA-256 = 32 bytes
    }

    // ==================== HMAC-SHA256 Tests ====================

    [Fact]
    public void ComputeHMACSHA256_ShouldReturnConsistentHash_WithSameKey()
    {
        // Arrange
        var input = "Message to authenticate";
        var key = "SecretKey123";

        // Act
        var hash1 = _service.ComputeHMACSHA256(input, key);
        var hash2 = _service.ComputeHMACSHA256(input, key);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // HMAC-SHA256 produces 32 bytes = 64 hex chars
    }

    [Fact]
    public void ComputeHMACSHA256_DifferentKeys_ShouldProduceDifferentHashes()
    {
        // Arrange
        var input = "Same message";

        // Act
        var hash1 = _service.ComputeHMACSHA256(input, "Key1");
        var hash2 = _service.ComputeHMACSHA256(input, "Key2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHMACSHA256_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Act
        var result = _service.ComputeHMACSHA256("", "key");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComputeHMACSHA256Bytes_ShouldReturnHash()
    {
        // Arrange
        var input = System.Text.Encoding.UTF8.GetBytes("Data");
        var key = System.Text.Encoding.UTF8.GetBytes("Key");

        // Act
        var hash = _service.ComputeHMACSHA256Bytes(input, key);

        // Assert
        Assert.Equal(32, hash.Length); // HMAC-SHA256 = 32 bytes
    }

    [Fact]
    public void ComputeHMACSHA256_ShouldBeDifferentFromSHA256()
    {
        // Arrange
        var input = "Test data";

        // Act
        var sha256 = _service.ComputeSHA256(input);
        var hmac = _service.ComputeHMACSHA256(input, "SomeKey");

        // Assert
        Assert.NotEqual(sha256, hmac);
    }
}
