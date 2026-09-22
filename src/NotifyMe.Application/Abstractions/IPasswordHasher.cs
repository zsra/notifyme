namespace NotifyMe.Application.Abstractions;

/// <summary>
/// Hashes and verifies end-user passwords. Implemented in Infrastructure using the .NET BCL's
/// PBKDF2 (see ADR-0010); Application never sees or handles a plaintext password beyond this
/// boundary.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}
