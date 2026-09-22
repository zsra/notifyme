using System.Net;
using System.Net.Http.Headers;
using NotifyMe.Api.Authentication;
using NotifyMe.IntegrationTests;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Verifies ADR-0006's "requests without a valid API key are rejected" requirement against the
/// real Api host.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ApiKeyAuthTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private readonly AdminApiWebApplicationFactory _factory;

    public ApiKeyAuthTests(AdminApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAlertRules_WithoutApiKeyHeader_Returns401()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/admin/alert-rules");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAlertRules_WithWrongApiKey_Returns401()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, "not-the-right-key");

        using var response = await client.GetAsync("/api/admin/alert-rules");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAlertRules_WithValidApiKey_Returns200()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);

        using var response = await client.GetAsync("/api/admin/alert-rules");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
