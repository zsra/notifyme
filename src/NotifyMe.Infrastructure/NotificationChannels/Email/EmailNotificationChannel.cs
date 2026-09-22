using Microsoft.Extensions.Options;
using MimeKit;
using NotifyMe.Domain.Abstractions;

namespace NotifyMe.Infrastructure.NotificationChannels.Email;

/// <summary>
/// Sends notifications by email via SMTP (see docs/architecture/notification-channels.md and
/// ADR-0005). Locally, this points at MailHog so delivery can be verified visually without a
/// real mailbox or credentials.
/// </summary>
public sealed class EmailNotificationChannel : INotificationChannel
{
    private readonly EmailChannelOptions _options;
    private readonly IEmailSender _emailSender;

    public EmailNotificationChannel(IOptions<EmailChannelOptions> options, IEmailSender emailSender)
    {
        _options = options.Value;
        _emailSender = emailSender;
    }

    public string ChannelType => "email";

    public async Task<NotificationDispatchResult> SendAsync(NotificationDispatchContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var message = BuildMessage(context);

        try
        {
            await _emailSender.SendAsync(message, cancellationToken);
            return NotificationDispatchResult.Success();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return NotificationDispatchResult.Failure($"Failed to send email: {ex.Message}");
        }
    }

    private MimeMessage BuildMessage(NotificationDispatchContext context)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(context.Channel.Target));
        message.Subject = $"[NotifyMe] {context.Rule.Name}: {context.Event.Title}";
        message.Body = new TextPart("plain")
        {
            Text = string.IsNullOrWhiteSpace(context.Event.Description)
                ? context.Event.Title
                : $"{context.Event.Title}\n\n{context.Event.Description}",
        };

        return message;
    }
}
