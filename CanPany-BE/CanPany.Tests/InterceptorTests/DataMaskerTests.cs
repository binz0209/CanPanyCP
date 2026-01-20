using CanPany.Infrastructure.Interceptors;
using Xunit;

namespace CanPany.Tests.InterceptorTests;

public class DataMaskerTests
{
    private readonly DataMasker _masker = new();

    [Fact]
    public void IsSensitiveKey_ShouldReturnTrue_ForPassword()
    {
        Assert.True(_masker.IsSensitiveKey("password"));
        Assert.True(_masker.IsSensitiveKey("Password"));
        Assert.True(_masker.IsSensitiveKey("PASSWORD"));
    }

    [Fact]
    public void IsSensitiveKey_ShouldReturnTrue_ForToken()
    {
        Assert.True(_masker.IsSensitiveKey("token"));
        Assert.True(_masker.IsSensitiveKey("access_token"));
        Assert.True(_masker.IsSensitiveKey("refresh_token"));
    }

    [Fact]
    public void IsSensitiveKey_ShouldReturnTrue_ForSecret()
    {
        Assert.True(_masker.IsSensitiveKey("secret"));
        Assert.True(_masker.IsSensitiveKey("client_secret"));
        Assert.True(_masker.IsSensitiveKey("api_secret"));
    }

    [Fact]
    public void IsSensitiveKey_ShouldReturnFalse_ForNormalKey()
    {
        Assert.False(_masker.IsSensitiveKey("username"));
        Assert.False(_masker.IsSensitiveKey("email"));
        Assert.False(_masker.IsSensitiveKey("name"));
    }

    [Fact]
    public void MaskSensitiveData_ShouldMaskSensitiveKeys()
    {
        var data = new Dictionary<string, object?>
        {
            ["username"] = "testuser",
            ["password"] = "secret123",
            ["token"] = "abc123",
            ["email"] = "test@example.com"
        };

        var masked = _masker.MaskSensitiveData(data);

        Assert.Equal("testuser", masked["username"]);
        Assert.Equal("***MASKED***", masked["password"]);
        Assert.Equal("***MASKED***", masked["token"]);
        Assert.Equal("test@example.com", masked["email"]);
    }

    [Fact]
    public void MaskSensitiveData_ShouldMaskLongTokenLikeStrings()
    {
        var data = new Dictionary<string, object?>
        {
            ["normalString"] = "short",
            ["tokenLike"] = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0"
        };

        var masked = _masker.MaskSensitiveData(data);

        Assert.Equal("short", masked["normalString"]);
        Assert.Equal("***MASKED***", masked["tokenLike"]);
    }
}
