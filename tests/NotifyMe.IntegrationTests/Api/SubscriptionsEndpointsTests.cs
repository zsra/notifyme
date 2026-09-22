using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Channels;
using NotifyMe.Application.Subscriptions;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.IntegrationTests;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Create/list/delete round trip for the <c>/api/admin/subscriptions</c> endpoints, which link
/// an alert rule to a channel (see docs/api/admin-api.md).
/// </summary>
[Collection(PostgresCollection.Name)]
public class SubscriptionsEndpointsTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public SubscriptionsEndpointsTests(AdminApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);
    }

    [Fact]
    public async Task CreateListDelete_RoundTripsASubscription()
    {
        var alertRule = await CreateAlertRuleAsync();
        var channel = await CreateChannelAsync();

        var createRequest = new { alertRuleId = alertRule.Id, channelConfigId = channel.Id };
        using var createResponse = await _client.PostAsJsonAsync("/api/admin/subscriptions", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<SubscriptionDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(alertRule.Id, created!.AlertRuleId);
        Assert.Equal(channel.Id, created.ChannelConfigId);

        using var listResponse = await _client.GetAsync($"/api/admin/subscriptions?alertRuleId={alertRule.Id}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<SubscriptionDto>>(JsonOptions);
        Assert.Contains(list!, subscription => subscription.Id == created.Id);

        using var deleteResponse = await _client.DeleteAsync($"/api/admin/subscriptions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await _client.DeleteAsync($"/api/admin/alert-rules/{alertRule.Id}");
        await _client.DeleteAsync($"/api/admin/channels/{channel.Id}");
    }

    private async Task<AlertRuleDto> CreateAlertRuleAsync()
    {
        var request = new
        {
            name = "Subscription test rule",
            category = EventCategory.BreakingNews,
            keywords = new[] { "outage" },
            minimumSeverity = Severity.Medium,
        };

        using var response = await _client.PostAsJsonAsync("/api/admin/alert-rules", request);
        return (await response.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions))!;
    }

    private async Task<ChannelConfigDto> CreateChannelAsync()
    {
        var request = new { channelType = "slack", target = "https://hooks.slack.com/services/PLACEHOLDER" };

        using var response = await _client.PostAsJsonAsync("/api/admin/channels", request);
        return (await response.Content.ReadFromJsonAsync<ChannelConfigDto>(JsonOptions))!;
    }
}
