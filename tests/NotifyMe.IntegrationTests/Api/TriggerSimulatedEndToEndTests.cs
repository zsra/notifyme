using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Channels;
using NotifyMe.Application.Events;
using NotifyMe.Application.Notifications;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.IntegrationTests;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Phase 08's end-to-end verification criterion: create a rule, trigger a simulated event, see
/// it land in notification history, and confirm delivery in WireMock (see
/// ai/plan/phase-08-api-layer.md). Uses one alert rule per <see cref="EventCategory"/> (each
/// with no keyword restriction) so whichever category the simulated fetch happens to produce is
/// guaranteed to match at least one of them, keeping the test deterministic despite
/// `SimulatedEventSource`'s randomness.
/// </summary>
[Collection(PostgresCollection.Name)]
public class TriggerSimulatedEndToEndTests : IClassFixture<AdminApiWebApplicationFactory>, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;
    private readonly WireMockServer _wireMockServer;

    public TriggerSimulatedEndToEndTests(AdminApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);
        _wireMockServer = WireMockServer.Start();
    }

    public void Dispose()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
    }

    [Fact]
    public async Task TriggerSimulated_DispatchesToSubscribedChannel_VisibleInNotificationHistoryAndWireMock()
    {
        const string webhookPath = "/services/T000/B000/END-TO-END";
        _wireMockServer
            .Given(Request.Create().WithPath(webhookPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("ok"));

        var channel = await CreateChannelAsync($"{_wireMockServer.Urls[0]}{webhookPath}");
        var alertRules = new List<AlertRuleDto>();

        foreach (var category in Enum.GetValues<EventCategory>())
        {
            var alertRule = await CreateAlertRuleAsync(category);
            alertRules.Add(alertRule);
            await SubscribeAsync(alertRule.Id, channel.Id);
        }

        using var triggerResponse = await _client.PostAsync("/api/admin/events/trigger-simulated", content: null);
        Assert.Equal(HttpStatusCode.OK, triggerResponse.StatusCode);

        var ingestResult = await triggerResponse.Content.ReadFromJsonAsync<IngestEventsResult>(JsonOptions);
        Assert.NotNull(ingestResult);
        Assert.True(ingestResult!.NotificationsDispatched > 0);

        using var notificationsResponse = await _client.GetAsync("/api/admin/notifications");
        Assert.Equal(HttpStatusCode.OK, notificationsResponse.StatusCode);
        var notifications = await notificationsResponse.Content.ReadFromJsonAsync<List<NotificationDto>>(JsonOptions);

        var ourNotifications = notifications!
            .Where(notification => alertRules.Any(rule => rule.Id == notification.AlertRuleId))
            .ToList();
        Assert.NotEmpty(ourNotifications);
        Assert.Contains(ourNotifications, notification => notification.Status == NotifyMe.Domain.Notifications.NotificationStatus.Sent);

        var sentNotification = ourNotifications.First(notification => notification.Status == NotifyMe.Domain.Notifications.NotificationStatus.Sent);
        using var getNotificationResponse = await _client.GetAsync($"/api/admin/notifications/{sentNotification.Id}");
        Assert.Equal(HttpStatusCode.OK, getNotificationResponse.StatusCode);

        Assert.NotEmpty(_wireMockServer.LogEntries);

        foreach (var alertRule in alertRules)
        {
            await _client.DeleteAsync($"/api/admin/alert-rules/{alertRule.Id}");
        }

        await _client.DeleteAsync($"/api/admin/channels/{channel.Id}");
    }

    private async Task<AlertRuleDto> CreateAlertRuleAsync(EventCategory category)
    {
        var request = new
        {
            name = $"End-to-end rule ({category})",
            category,
            keywords = Array.Empty<string>(),
            minimumSeverity = Severity.Medium,
        };

        using var response = await _client.PostAsJsonAsync("/api/admin/alert-rules", request);
        return (await response.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions))!;
    }

    private async Task<ChannelConfigDto> CreateChannelAsync(string webhookUrl)
    {
        var request = new { channelType = "slack", target = webhookUrl };

        using var response = await _client.PostAsJsonAsync("/api/admin/channels", request);
        return (await response.Content.ReadFromJsonAsync<ChannelConfigDto>(JsonOptions))!;
    }

    private async Task SubscribeAsync(Guid alertRuleId, Guid channelConfigId)
    {
        var request = new { alertRuleId, channelConfigId };
        using var response = await _client.PostAsJsonAsync("/api/admin/subscriptions", request);
        response.EnsureSuccessStatusCode();
    }
}
