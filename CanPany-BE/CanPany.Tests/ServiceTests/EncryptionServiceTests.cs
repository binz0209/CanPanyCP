using CanPany.Infrastructure.Security.Encryption;
using Xunit;

namespace CanPany.Tests.ServiceTests;

public class EncryptionServiceTests
{
    private readonly EncryptionService _service = new();
    private const string TestKey = "TestEncryptionKey-MustBeAtLeast32Characters!";

    [Fact]
    public void Encrypt_Decrypt_ShouldRoundTrip_String()
    {
        // Arrange
        var plainText = "Hello, this is a sensitive message!";

        // Act
        var encrypted = _service.Encrypt(plainText, TestKey);
        var decrypted = _service.Decrypt(encrypted, TestKey);

        // Assert
        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_Decrypt_ShouldRoundTrip_Bytes()
    {
        // Arrange
        var data = System.Text.Encoding.UTF8.GetBytes("Binary data for encryption test");

        // Act
        var encrypted = _service.EncryptBytes(data, TestKey);
        var decrypted = _service.DecryptBytes(encrypted, TestKey);

        // Assert
        Assert.NotEqual(data, encrypted);
        Assert.Equal(data, decrypted);
    }

    [Fact]
    public void Encrypt_ShouldReturnNull_WhenInputIsNull()
    {
        // Act & Assert
        Assert.Null(_service.Encrypt(null!, TestKey));
    }

    [Fact]
    public void Encrypt_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Act
        var result = _service.Encrypt("", TestKey);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentCiphertext_EachTime()
    {
        // Arrange (random salt means different ciphertext each time)
        var plainText = "Same input, different output";

        // Act
        var encrypted1 = _service.Encrypt(plainText, TestKey);
        var encrypted2 = _service.Encrypt(plainText, TestKey);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2);
        // But both should decrypt to the same value
        Assert.Equal(plainText, _service.Decrypt(encrypted1, TestKey));
        Assert.Equal(plainText, _service.Decrypt(encrypted2, TestKey));
    }

    [Fact]
    public void Decrypt_ShouldFail_WithWrongKey()
    {
        // Arrange
        var plainText = "Secret data";
        var encrypted = _service.Encrypt(plainText, TestKey);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            _service.Decrypt(encrypted, "WrongKey-ThatIsAlsoAtLeast32Characters!!"));
    }

    [Fact]
    public void Encrypt_ShouldHandleUnicode()
    {
        // Arrange
        var plainText = "Xin chào! 你好! こんにちは! 🎉";

        // Act
        var encrypted = _service.Encrypt(plainText, TestKey);
        var decrypted = _service.Decrypt(encrypted, TestKey);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void EncryptBytes_ShouldReturnEmpty_WhenInputIsEmpty()
    {
        // Act
        var result = _service.EncryptBytes(Array.Empty<byte>(), TestKey);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Encrypt_Decrypt_ShouldHandleLongText()
    {
        // Arrange
        var plainText = new string('A', 10000);

        // Act
        var encrypted = _service.Encrypt(plainText, TestKey);
        var decrypted = _service.Decrypt(encrypted, TestKey);

        // Assert
        Assert.Equal(plainText, decrypted);
    }
}
