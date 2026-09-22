using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NotifyMe.Api.Authentication;
using NotifyMe.Application.AlertRules;
using NotifyMe.Application.Channels;
using NotifyMe.Application.Subscriptions;
using NotifyMe.Application.Users;
using NotifyMe.Domain.Common;
using NotifyMe.Domain.Events;
using NotifyMe.IntegrationTests;
using Xunit;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// End-to-end coverage of the Phase 16 self-service surface (see ADR-0010): register, log in,
/// create an alert rule/channel/subscription via <c>/api/me/*</c>, and confirm ownership
/// isolation, another user (and the Admin API) cannot see or modify a user's own rows, but the
/// Admin API's own visibility is unaffected.
/// </summary>
[Collection(PostgresCollection.Name)]
public class MyEndpointsTests : IClassFixture<AdminApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly AdminApiWebApplicationFactory _factory;

    public MyEndpointsTests(AdminApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, AuthResultDto Auth)> RegisterAndAuthenticateAsync(string email)
    {
        var client = _factory.CreateClient();
        using var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register", new { email, password = "a-strong-password" });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return (client, auth);
    }

    [Fact]
    public async Task Register_ThenLogin_ReturnsMatchingUser()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        var (_, registerAuth) = await RegisterAndAuthenticateAsync(email);

        var loginClient = _factory.CreateClient();
        using var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/auth/login", new { email, password = "a-strong-password" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginAuth = await loginResponse.Content.ReadFromJsonAsync<AuthResultDto>(JsonOptions);
        Assert.Equal(registerAuth.User.Id, loginAuth!.User.Id);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await RegisterAndAuthenticateAsync(email);

        var loginClient = _factory.CreateClient();
        using var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/auth/login", new { email, password = "wrong-password" });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task MyAlertRules_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        using var response = await client.GetAsync("/api/me/alert-rules");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAlertRuleChannelAndSubscription_ViaMyEndpoints_RoundTrips()
    {
        var (client, _) = await RegisterAndAuthenticateAsync($"{Guid.NewGuid()}@example.com");

        using var ruleResponse = await client.PostAsJsonAsync("/api/me/alert-rules", new
        {
            name = "My rule",
            category = EventCategory.MarketMovement,
            keywords = new[] { "index" },
            minimumSeverity = Severity.Medium,
            isEnabled = true,
        });
        Assert.Equal(HttpStatusCode.Created, ruleResponse.StatusCode);
        var rule = await ruleResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);

        using var channelResponse = await client.PostAsJsonAsync("/api/me/channels", new
        {
            channelType = "email",
            target = "me@example.com",
            isEnabled = true,
        });
        Assert.Equal(HttpStatusCode.Created, channelResponse.StatusCode);
        var channel = await channelResponse.Content.ReadFromJsonAsync<ChannelConfigDto>(JsonOptions);

        using var subscriptionResponse = await client.PostAsJsonAsync(
            "/api/me/subscriptions", new { alertRuleId = rule!.Id, channelConfigId = channel!.Id });
        Assert.Equal(HttpStatusCode.Created, subscriptionResponse.StatusCode);
        var subscription = await subscriptionResponse.Content.ReadFromJsonAsync<SubscriptionDto>(JsonOptions);
        Assert.Equal(rule.Id, subscription!.AlertRuleId);

        using var listResponse = await client.GetAsync("/api/me/alert-rules");
        var list = await listResponse.Content.ReadFromJsonAsync<List<AlertRuleDto>>(JsonOptions);
        Assert.Contains(list!, r => r.Id == rule.Id);
    }

    [Fact]
    public async Task AnotherUser_CannotSeeOrModifyFirstUsersAlertRule()
    {
        var (ownerClient, _) = await RegisterAndAuthenticateAsync($"{Guid.NewGuid()}@example.com");
        using var createResponse = await ownerClient.PostAsJsonAsync("/api/me/alert-rules", new
        {
            name = "Owner-only rule",
            category = EventCategory.NaturalDisaster,
            keywords = new[] { "flood" },
            minimumSeverity = Severity.Low,
            isEnabled = true,
        });
        var owned = await createResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);

        var (otherClient, _) = await RegisterAndAuthenticateAsync($"{Guid.NewGuid()}@example.com");

        using var getResponse = await otherClient.GetAsync($"/api/me/alert-rules/{owned!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using var listResponse = await otherClient.GetAsync("/api/me/alert-rules");
        var list = await listResponse.Content.ReadFromJsonAsync<List<AlertRuleDto>>(JsonOptions);
        Assert.DoesNotContain(list!, r => r.Id == owned.Id);

        using var deleteResponse = await otherClient.DeleteAsync($"/api/me/alert-rules/{owned.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AdminApi_SeesUserCreatedAlertRule_ButMyEndpointDoesNotSeeAdminCreatedOne()
    {
        var (userClient, _) = await RegisterAndAuthenticateAsync($"{Guid.NewGuid()}@example.com");
        using var userCreateResponse = await userClient.PostAsJsonAsync("/api/me/alert-rules", new
        {
            name = "User rule visible to admin",
            category = EventCategory.MarketMovement,
            keywords = new[] { "index" },
            minimumSeverity = Severity.Medium,
            isEnabled = true,
        });
        var userRule = await userCreateResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);

        var adminClient = _factory.CreateClient();
        adminClient.DefaultRequestHeaders.Add(ApiKeyEndpointFilter.HeaderName, AdminApiWebApplicationFactory.ApiKey);

        using var adminGetResponse = await adminClient.GetAsync($"/api/admin/alert-rules/{userRule!.Id}");
        Assert.Equal(HttpStatusCode.OK, adminGetResponse.StatusCode);

        using var adminCreateResponse = await adminClient.PostAsJsonAsync("/api/admin/alert-rules", new
        {
            name = "Admin-only rule",
            category = EventCategory.MarketMovement,
            keywords = new[] { "rate" },
            minimumSeverity = Severity.Medium,
            isEnabled = true,
        });
        var adminRule = await adminCreateResponse.Content.ReadFromJsonAsync<AlertRuleDto>(JsonOptions);

        using var userListResponse = await userClient.GetAsync("/api/me/alert-rules");
        var userList = await userListResponse.Content.ReadFromJsonAsync<List<AlertRuleDto>>(JsonOptions);
        Assert.DoesNotContain(userList!, r => r.Id == adminRule!.Id);
    }
}
