using System.Text.Json;
using Microsoft.Extensions.Options;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.NotificationChannels.Email;
using Xunit;

namespace NotifyMe.IntegrationTests.NotificationChannels;

/// <summary>
/// Phase 07's "manual/integration check" for email: sends a real message via
/// <see cref="EmailNotificationChannel"/>/<see cref="SmtpEmailSender"/> over SMTP to the local
/// docker-compose MailHog instance, then confirms it actually arrived by querying MailHog's own
/// HTTP API (see ADR-0005/ADR-0007). Requires `docker compose up -d mailhog` first.
/// </summary>
public class EmailNotificationChannelMailHogTests
{
    private const string MailHogBaseUrl = "http://localhost:8025";

    [Fact]
    public async Task SendAsync_DeliversRealEmail_VisibleInMailHog()
    {
        var options = Options.Create(new EmailChannelOptions
        {
            SmtpHost = "localhost",
            SmtpPort = 1025,
            UseSsl = false,
            FromAddress = "notifyme@example.com",
            FromName = "NotifyMe",
        });
        var channel = new EmailNotificationChannel(options, new SmtpEmailSender(options));
        var recipient = $"phase07-check-{Guid.NewGuid():N}@example.com";
        var context = CreateContext(recipient);

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);

        using var httpClient = new HttpClient { BaseAddress = new Uri(MailHogBaseUrl) };
        using var response = await httpClient.GetAsync($"/api/v2/search?kind=to&query={Uri.EscapeDataString(recipient)}");
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = document.RootElement.GetProperty("items");

        Assert.True(items.GetArrayLength() > 0, $"Expected MailHog to have received a message addressed to {recipient}");

        var subject = items[0].GetProperty("Content").GetProperty("Headers").GetProperty("Subject")[0].GetString();
        Assert.Contains("Flood watch", subject);
    }

    private static NotificationDispatchContext CreateContext(string toAddress)
    {
        var criteria = MatchCriteria.Create(new[] { "flood" }, Severity.Medium);
        var rule = AlertRule.Create(Guid.NewGuid(), "Flood watch", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);
        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.NaturalDisaster, "Flash flood warning", "Multiple river basins affected", Severity.High, DateTimeOffset.UtcNow);
        var channel = ChannelConfig.Create(Guid.NewGuid(), "email", toAddress);

        return new NotificationDispatchContext(rule, normalizedEvent, channel);
    }
}
