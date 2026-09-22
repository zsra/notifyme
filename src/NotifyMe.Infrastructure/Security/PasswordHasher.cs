using System.Security.Cryptography;
using NotifyMe.Application.Abstractions;

namespace NotifyMe.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing using the .NET BCL directly (no external dependency, see
/// ADR-0010). Stored format is <c>{iterations}.{base64 salt}.{base64 hash}</c> so the iteration
/// count can be increased later without invalidating hashes already stored with a lower count.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
