using Microsoft.EntityFrameworkCore;
using NotifyMe.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace NotifyMe.IntegrationTests;

/// <summary>
/// Starts one ephemeral PostgreSQL container (via Testcontainers) for the whole integration test
/// run and applies EF Core migrations against it once. Shared across every test class in the
/// <see cref="PostgresCollection"/> collection via xUnit's collection-fixture mechanism, so the
/// container is created once and torn down once, not per test class.
///
/// This replaces the previous dependency on a fixed local `docker compose up -d postgres`
/// instance on a hardcoded port (Phase 05/09 era): `dotnet test` now works against a clean,
/// isolated database with no manual setup beyond having a Docker daemon reachable (see Phase 11
/// plan).
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("notifyme")
            .WithUsername("notifyme")
            .WithPassword("notifyme_test_only")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var dbContext = new NotifyMeDbContext(
            new DbContextOptionsBuilder<NotifyMeDbContext>().UseNpgsql(ConnectionString).Options);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

/// <summary>
/// Groups every integration test class that needs the shared <see cref="PostgresContainerFixture"/>
/// so xUnit creates a single container for the whole run instead of one per test class.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Postgres";
}
