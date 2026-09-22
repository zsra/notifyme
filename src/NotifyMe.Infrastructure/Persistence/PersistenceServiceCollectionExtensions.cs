using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Infrastructure.Persistence.Repositories;

namespace NotifyMe.Infrastructure.Persistence;

/// <summary>
/// Wires the Npgsql provider and the repository implementations for the connection string
/// resolved (in priority order: `NOTIFYME_CONNECTION_STRING`, then the `ConnectionStrings:Postgres`
/// configuration key, then a local-dev fallback) from the supplied <see cref="IConfiguration"/>.
///
/// The connection string is deliberately resolved lazily inside the `AddDbContext` options
/// delegate (evaluated at DI-resolution time) rather than eagerly by the caller before this
/// method runs: `WebApplicationFactory`-based integration tests override configuration via
/// `ConfigureAppConfiguration`, but that override is only merged into the final `IConfiguration`
/// during `WebApplicationBuilder.Build()` - code that reads configuration eagerly beforehand
/// (as the Api composition root used to) never sees the test's overridden connection string.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyMePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotifyMeDbContext>(options => options.UseNpgsql(ResolveConnectionString(configuration)));

        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IChannelConfigRepository, ChannelConfigRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration) =>
        configuration["NOTIFYME_CONNECTION_STRING"]
        ?? configuration.GetConnectionString("Postgres")
        ?? "Host=localhost;Port=5432;Database=notifyme;Username=notifyme;Password=notifyme_dev_only";
}
