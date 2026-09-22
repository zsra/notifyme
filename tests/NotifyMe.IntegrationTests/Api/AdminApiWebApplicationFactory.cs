using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace NotifyMe.IntegrationTests.Api;

/// <summary>
/// Spins up the real Api host (including the real Postgres-backed persistence and real
/// notification channels) via <see cref="WebApplicationFactory{TEntryPoint}"/>, overriding only
/// the connection string and Admin API key so tests don't depend on `dotnet user-secrets` being
/// configured locally. Requires the local docker-compose Postgres service to be running first:
/// `docker compose up -d postgres`.
/// </summary>
public sealed class AdminApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "integration-test-admin-key";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] =
                    "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only",
                ["Admin:ApiKey"] = ApiKey,
            });
        });
    }
}
