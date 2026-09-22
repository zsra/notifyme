using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace NotifyMe.Infrastructure.NotificationChannels.Email;

/// <summary>
/// The actual SMTP conversation, isolated behind this thin seam so
/// <see cref="EmailNotificationChannel"/>'s message-building and error-handling logic can be
/// unit tested with a hand-rolled fake instead of a real SMTP server (see ADR-0005/ADR-0007).
/// </summary>
public interface IEmailSender
{
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken);
}

/// <summary>
/// Real implementation: connects to the configured SMTP server (MailHog locally) via MailKit,
/// sends the message, and disconnects. A fresh <see cref="SmtpClient"/> is created per call so
/// this type has no shared mutable state and is safe to register as a singleton.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailChannelOptions _options;

    public SmtpEmailSender(IOptions<EmailChannelOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None,
            cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
