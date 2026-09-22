namespace NotifyMe.Infrastructure.NotificationChannels.Email;

/// <summary>
/// Configuration for <see cref="EmailNotificationChannel"/> (via <see cref="SmtpEmailSender"/>),
/// bound from the <c>NotificationChannels:Email</c> appsettings section. Locally this points at
/// MailHog (run via docker-compose); see ADR-0005.
/// </summary>
public sealed class EmailChannelOptions
{
    public const string SectionName = "NotificationChannels:Email";

    public string SmtpHost { get; set; } = "localhost";

    public int SmtpPort { get; set; } = 1025;

    /// <summary>
    /// MailHog does not support TLS, so this defaults to false for local development. A real
    /// SMTP provider in production would need this set to true.
    /// </summary>
    public bool UseSsl { get; set; }

    public string FromAddress { get; set; } = "notifyme@example.com";

    public string FromName { get; set; } = "NotifyMe";
}
