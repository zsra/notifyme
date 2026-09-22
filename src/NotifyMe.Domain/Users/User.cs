using NotifyMe.Domain.Common;

namespace NotifyMe.Domain.Users;

/// <summary>
/// An end user who can register and log in to manage their own <see cref="Alerts.AlertRule"/>s,
/// <see cref="Channels.ChannelConfig"/>s, and <see cref="Subscriptions.Subscription"/>s via the
/// self-service `/api/me` surface (Phase 16), as distinct from the Admin API's single shared
/// API key. See ADR-0010 for why this is a flat, single-role user rather than a full
/// roles/permissions system.
/// </summary>
public sealed class User : Entity
{
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// The PBKDF2 hash produced by the Infrastructure-layer password hasher, never a plaintext
    /// password. Opaque to Domain/Application beyond "compare it via the hasher."
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private User()
    {
    }

    private User(Guid id, string email, string passwordHash, DateTimeOffset createdAt)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public static User Create(Guid id, string email, string passwordHash, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        return new User(id, Normalize(email), passwordHash, createdAt);
    }

    /// <summary>
    /// Emails are matched case-insensitively at the repository layer; normalizing here too keeps
    /// what's stored/displayed consistent with what's matched against.
    /// </summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
