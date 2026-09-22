using Microsoft.EntityFrameworkCore;
using NotifyMe.Domain.Alerts;
using NotifyMe.Domain.Channels;
using NotifyMe.Domain.Notifications;
using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Infrastructure.Persistence;

/// <summary>
/// The only persisted entities are the ones the Application layer's repository interfaces
/// actually need (see ai/plan/phase-04-application-layer.md's "design notes"): alert rules,
/// subscriptions, channel configs, and notifications. Raw/normalized events are not persisted -
/// they only exist for the duration of one ingestion pass.
/// </summary>
public sealed class NotifyMeDbContext : DbContext
{
    public NotifyMeDbContext(DbContextOptions<NotifyMeDbContext> options)
        : base(options)
    {
    }

    public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<ChannelConfig> ChannelConfigs => Set<ChannelConfig>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotifyMeDbContext).Assembly);
    }
}
