using NotifyMe.Infrastructure.Security;
using Xunit;

namespace NotifyMe.Infrastructure.Tests.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.HashPassword("correct-horse-battery-staple");

        Assert.True(_hasher.VerifyPassword("correct-horse-battery-staple", hash));
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.HashPassword("correct-horse-battery-staple");

        Assert.False(_hasher.VerifyPassword("wrong-password", hash));
    }

    [Fact]
    public void HashPassword_ForSamePassword_ProducesDifferentHashesEachTime()
    {
        var hash1 = _hasher.HashPassword("same-password");
        var hash2 = _hasher.HashPassword("same-password");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.VerifyPassword("same-password", hash1));
        Assert.True(_hasher.VerifyPassword("same-password", hash2));
    }
}
