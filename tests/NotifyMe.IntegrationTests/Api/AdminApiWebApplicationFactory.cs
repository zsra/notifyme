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
///
/// Also stretches the simulated event source's polling interval out to 1 day so the real
/// `EventIngestionWorker` (Phase 09, runs automatically on host startup) never fires a second,
/// uncontrolled ingestion pass mid-test-run; every test triggers ingestion explicitly via
/// `POST /api/admin/events/trigger-simulated` instead.
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
                ["EventIngestion:Simulated:PollingInterval"] = "1.00:00:00",
            });
        });
    }
}
