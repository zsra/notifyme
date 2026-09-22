using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.Channels;
using NotifyMe.IntegrationTests;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// CRUD round trip for the <c>/api/admin/channels</c> endpoints against the real Api host and a
/// real Postgres instance (see docs/api/admin-api.md).
/// </summary>
[Collection(PostgresCollection.Name)]
public class ChannelsEndpointsTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public ChannelsEndpointsTests(AdminApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);
    }

    [Fact]
    public async Task CreateGetUpdateListDelete_RoundTripsAChannel()
    {
        var createRequest = new { channelType = "slack", target = "https://hooks.slack.com/services/PLACEHOLDER", isEnabled = true };

        using var createResponse = await _client.PostAsJsonAsync("/api/admin/channels", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ChannelConfigDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("slack", created!.ChannelType);

        using var getResponse = await _client.GetAsync($"/api/admin/channels/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var listResponse = await _client.GetAsync("/api/admin/channels?channelType=slack");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<ChannelConfigDto>>(JsonOptions);
        Assert.Contains(list!, channel => channel.Id == created.Id);

        var updateRequest = new { channelType = "slack", target = "https://hooks.slack.com/services/UPDATED" };

        using var updateResponse = await _client.PutAsJsonAsync($"/api/admin/channels/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ChannelConfigDto>(JsonOptions);
        Assert.Equal("https://hooks.slack.com/services/UPDATED", updated!.Target);

        using var deleteResponse = await _client.DeleteAsync($"/api/admin/channels/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getAfterDeleteResponse = await _client.GetAsync($"/api/admin/channels/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }
}
