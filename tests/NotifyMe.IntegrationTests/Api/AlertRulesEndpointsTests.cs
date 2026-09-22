using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.AlertRules;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// CRUD round trip for the <c>/api/admin/alert-rules</c> endpoints against the real Api host and
/// a real Postgres instance (see docs/api/admin-api.md).
/// </summary>
public class AlertRulesEndpointsTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public AlertRulesEndpointsTests(AdminApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);
    }

    [Fact]
    public async Task CreateGetUpdateListDelete_RoundTripsAnAlertRule()
    {
        var createRequest = new
        {
            name = "Integration test rule",
            category = EventCategory.MarketMovement,
            keywords = new[] { "index" },
            minimumSeverity = Severity.Medium,
            isEnabled = true,
        };

        using var createResponse = await _client.PostAsJsonAsync("/api/admin/alert-rules", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Integration test rule", created!.Name);

        using var getResponse = await _client.GetAsync($"/api/admin/alert-rules/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var listResponse = await _client.GetAsync($"/api/admin/alert-rules?eventCategory={EventCategory.MarketMovement}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<AlertRuleDto>>(JsonOptions);
        Assert.Contains(list!, rule => rule.Id == created.Id);

        var updateRequest = new
        {
            name = "Integration test rule (updated)",
            category = EventCategory.MarketMovement,
            keywords = new[] { "index", "rate" },
            minimumSeverity = Severity.High,
        };

        using var updateResponse = await _client.PutAsJsonAsync($"/api/admin/alert-rules/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);
        Assert.Equal("Integration test rule (updated)", updated!.Name);
        Assert.Equal(Severity.High, updated.MinimumSeverity);

        using var deleteResponse = await _client.DeleteAsync($"/api/admin/alert-rules/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getAfterDeleteResponse = await _client.GetAsync($"/api/admin/alert-rules/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAlertRule_WithMissingName_Returns400()
    {
        var invalidRequest = new
        {
            name = "",
            category = EventCategory.BreakingNews,
            keywords = new[] { "something" },
            minimumSeverity = Severity.Medium,
        };

        using var response = await _client.PostAsJsonAsync("/api/admin/alert-rules", invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
