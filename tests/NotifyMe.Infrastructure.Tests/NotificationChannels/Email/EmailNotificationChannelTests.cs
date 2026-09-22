using Microsoft.Extensions.Options;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.NotificationChannels.Email;
using Xunit;

namespace NotifyMe.Infrastructure.Tests.NotificationChannels.Email;

public class EmailNotificationChannelTests
{
    private static NotificationDispatchContext CreateContext(string toAddress)
    {
        var criteria = MatchCriteria.Create(new[] { "flood" }, Severity.Medium);
        var rule = AlertRule.Create(Guid.NewGuid(), "Flood watch", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);
        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.NaturalDisaster, "Flash flood warning", "Multiple river basins affected", Severity.High, DateTimeOffset.UtcNow);
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", toAddress);

        return new NotificationDispatchContext(rule, normalizedEvent, channel);
    }

    private static EmailNotificationChannel CreateChannel(FakeEmailSender sender) =>
        new(Options.Create(new EmailChannelOptions { FromAddress = "alerts@notifyme.example", FromName = "NotifyMe Alerts" }), sender);

    [Fact]
    public void ChannelType_IsEmail()
    {
        var channel = CreateChannel(new FakeEmailSender());

        Assert.Equal("email", channel.ChannelType);
    }

    [Fact]
    public async Task SendAsync_WhenSenderSucceeds_BuildsMessageAndReturnsSuccess()
    {
        var sender = new FakeEmailSender();
        var channel = CreateChannel(sender);
        var context = CreateContext("subscriber@example.com");

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, sender.SendCallCount);
        Assert.NotNull(sender.LastMessage);
        Assert.Equal("subscriber@example.com", sender.LastMessage!.To.Mailboxes.Single().Address);
        Assert.Equal("alerts@notifyme.example", sender.LastMessage.From.Mailboxes.Single().Address);
        Assert.Contains("Flood watch", sender.LastMessage.Subject);
        Assert.Contains("Flash flood warning", sender.LastMessage.Subject);
    }

    [Fact]
    public async Task SendAsync_WhenSenderThrows_ReturnsFailureWithExceptionMessage()
    {
        var sender = new FakeEmailSender(new InvalidOperationException("smtp connection refused"));
        var channel = CreateChannel(sender);
        var context = CreateContext("subscriber@example.com");

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("smtp connection refused", result.ErrorMessage);
    }
}
