using NotifyMe.Domain.Users;
using Xunit;

namespace NotifyMe.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_NormalizesEmailToLowercaseAndTrimmed()
    {
        var user = User.Create(Guid.NewGuid(), "  Someone@Example.com  ", "hash", DateTimeOffset.UtcNow);

        Assert.Equal("someone@example.com", user.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankEmail_Throws(string? email)
    {
        var act = () => User.Create(Guid.NewGuid(), email!, "hash", DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankPasswordHash_Throws(string? passwordHash)
    {
        var act = () => User.Create(Guid.NewGuid(), "someone@example.com", passwordHash!, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(act);
    }
}
