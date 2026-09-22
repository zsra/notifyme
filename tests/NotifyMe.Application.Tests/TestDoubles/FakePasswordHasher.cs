using NotifyMe.Application.Abstractions;

namespace NotifyMe.Application.Tests.TestDoubles;

/// <summary>
/// A deliberately trivial (not cryptographically real) fake: Application-layer tests only need
/// to know "hashing happened" and "verification checks the hash," not a real KDF. The real
/// implementation is covered by NotifyMe.Infrastructure.Tests.Security.PasswordHasherTests.
/// </summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed:{password}";

    public bool VerifyPassword(string password, string passwordHash) => passwordHash == $"hashed:{password}";
}
