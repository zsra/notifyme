using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotifyMe.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add`/`dotnet ef database update` create a
/// <see cref="NotifyMeDbContext"/> at design time without needing the full Api host/DI
/// composition (which isn't wired until Phase 08). The connection string here matches the local
/// docker-compose Postgres service (see docker-compose.yml) - not a real secret, just the
/// shared local dev default.
/// </summary>
public sealed class NotifyMeDbContextFactory : IDesignTimeDbContextFactory<NotifyMeDbContext>
{
    public NotifyMeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NOTIFYME_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only";

        var optionsBuilder = new DbContextOptionsBuilder<NotifyMeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new NotifyMeDbContext(optionsBuilder.Options);
    }
}
