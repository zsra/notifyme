using System.Net;
using NotifyMe.IntegrationTests;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Verifies Phase 10's `/health` endpoint: reachable without an API key (it's an operational
/// probe, not an admin action) and healthy when the real Postgres dependency is up (see
/// ai/plan/phase-10-cross-cutting-concerns.md).
/// </summary>
[Collection(PostgresCollection.Name)]
public class HealthEndpointTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private readonly AdminApiWebApplicationFactory _factory;

    public HealthEndpointTests(AdminApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_WithoutApiKey_ReturnsHealthy()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
