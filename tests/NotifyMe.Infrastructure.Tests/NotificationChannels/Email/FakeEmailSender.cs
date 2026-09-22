using MimeKit;
using NotifyMe.Infrastructure.NotificationChannels.Email;

namespace NotifyMe.Infrastructure.Tests.NotificationChannels.Email;

/// <summary>
/// Hand-rolled fake <see cref="IEmailSender"/> so <see cref="Infrastructure.NotificationChannels.Email.EmailNotificationChannel"/>
/// can be unit tested without a real SMTP server.
/// </summary>
internal sealed class FakeEmailSender : IEmailSender
{
    private readonly Exception? _exceptionToThrow;

    public FakeEmailSender(Exception? exceptionToThrow = null)
    {
        _exceptionToThrow = exceptionToThrow;
    }

    public MimeMessage? LastMessage { get; private set; }

    public int SendCallCount { get; private set; }

    public Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        SendCallCount++;
        LastMessage = message;

        return _exceptionToThrow is null ? Task.CompletedTask : Task.FromException(_exceptionToThrow);
    }
}
