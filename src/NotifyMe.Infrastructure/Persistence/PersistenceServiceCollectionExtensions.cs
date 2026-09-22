using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotifyMe.Application.Abstractions.Repositories;
using NotifyMe.Infrastructure.Persistence.Repositories;

namespace NotifyMe.Infrastructure.Persistence;

/// <summary>
/// Wires the Npgsql provider and the repository implementations for the connection string the
/// caller (the Api composition root, from Phase 08 onward) supplies.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddNotifyMePersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotifyMeDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IChannelConfigRepository, ChannelConfigRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
