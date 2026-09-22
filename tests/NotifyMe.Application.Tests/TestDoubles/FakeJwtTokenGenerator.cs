using NotifyMe.Application.Abstractions;
using NotifyMe.Domain.Users;

namespace NotifyMe.Application.Tests.TestDoubles;

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user) =>
        ($"fake-token-for-{user.Id}", DateTimeOffset.UtcNow.AddHours(1));
}
