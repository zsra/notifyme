using System.Net;
using System.Text.Json;
using NotifyMe.Domain.Abstractions;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.Infrastructure.NotificationChannels.Slack;
using Xunit;

namespace NotifyMe.Infrastructure.Tests.NotificationChannels.Slack;

public class SlackNotificationChannelTests
{
    private static NotificationDispatchContext CreateContext(string webhookUrl)
    {
        var criteria = MatchCriteria.Create(new[] { "earthquake" }, Severity.Medium);
        var rule = AlertRule.Create(Guid.NewGuid(), "Disaster watch", EventCategory.NaturalDisaster, criteria, DateTimeOffset.UtcNow);
        var normalizedEvent = NormalizedEvent.Create(
            Guid.NewGuid(), Guid.NewGuid(), EventCategory.NaturalDisaster, "Magnitude 6.1 earthquake", "Tsunami watch issued", Severity.High, DateTimeOffset.UtcNow);
        var channel = ChannelConfig.Create(Guid.NewGuid(), "slack", webhookUrl);

        return new NotificationDispatchContext(rule, normalizedEvent, channel);
    }

    [Fact]
    public void ChannelType_IsSlack()
    {
        var channel = new SlackNotificationChannel(new HttpClient(new FakeHttpMessageHandler()));

        Assert.Equal("slack", channel.ChannelType);
    }

    [Fact]
    public async Task SendAsync_OnSuccessResponse_PostsPayloadToWebhookUrlAndReturnsSuccess()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var channel = new SlackNotificationChannel(new HttpClient(handler));
        var context = CreateContext("https://hooks.slack.com/services/T000/B000/PLACEHOLDER");

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://hooks.slack.com/services/T000/B000/PLACEHOLDER", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);

        using var body = JsonDocument.Parse(handler.LastRequestBody!);
        var text = body.RootElement.GetProperty("text").GetString();
        Assert.Contains("Disaster watch", text);
        Assert.Contains("Magnitude 6.1 earthquake", text);
    }

    [Fact]
    public async Task SendAsync_OnErrorResponse_ReturnsFailureWithStatusCodeInMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "invalid_payload");
        var channel = new SlackNotificationChannel(new HttpClient(handler));
        var context = CreateContext("https://hooks.slack.com/services/T000/B000/PLACEHOLDER");

        var result = await channel.SendAsync(context, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("400", result.ErrorMessage);
        Assert.Contains("invalid_payload", result.ErrorMessage);
    }
}
