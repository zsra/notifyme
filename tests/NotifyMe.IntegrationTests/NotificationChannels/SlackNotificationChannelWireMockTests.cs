using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.NotificationChannels.Slack;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NotifyMe.IntegrationTests.NotificationChannels;

/// <summary>
/// Phase 07's "manual/integration check" for Slack: posts a real HTTP request via
/// <see cref="SlackNotificationChannel"/> to a local WireMock.Net server standing in for a
/// Slack Incoming Webhook, and confirms the request was actually received with the expected
/// payload (see ADR-0005/ADR-0007). A fuller WireMock-based suite is planned for Phase 11.
/// </summary>
public class SlackNotificationChannelWireMockTests : IDisposable
{
    private readonly WireMockServer _server;

    public SlackNotificationChannelWireMockTests()
    {
        _server = WireMockServer.Start();
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    [Fact]
    public async Task SendAsync_PostsExpectedPayload_CapturedByWireMock()
    {
        const string webhookPath = "/services/T000/B000/PLACEHOLDER";

        _server
            .Given(Request.Create().WithPath(webhookPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("ok"));

        var channel = new SlackNotificationChannel(new HttpClient());
        var context = CreateContext($"{_server.Urls[0]}{webhookPath}");

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var logEntry = Assert.Single(_server.LogEntries);
        Assert.Equal("POST", logEntry.RequestMessage.Method);
        Assert.Contains("Disaster watch", logEntry.RequestMessage.Body);
        Assert.Contains("Magnitude 6.1 earthquake", logEntry.RequestMessage.Body);
    }

    private static NotificationDispatchContext CreateContext(string webhookUrl)
    {
        var criteria = MatchCriteria.Create(new[] { "earthquake" }, Severity.Medium);
        var rule = AlertRule.Create(Guid.NewGuid(), "Disaster watch", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);
        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.NaturalDisaster, "Magnitude 6.1 earthquake", "Tsunami watch issued", Severity.High, DateTimeOffset.UtcNow);
        var channel = ChannelConfig.Create(Guid.NewGuid(), "slack", webhookUrl);

        return new NotificationDispatchContext(rule, normalizedEvent, channel);
    }
}
