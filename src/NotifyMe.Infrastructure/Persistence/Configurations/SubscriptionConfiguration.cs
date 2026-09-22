using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotifyMe.Domain.Subscriptions;

namespace NotifyMe.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(subscription => subscription.Id);

        builder.Property(subscription => subscription.AlertRuleId).IsRequired();
        builder.Property(subscription => subscription.ChannelConfigId).IsRequired();

        builder.HasIndex(subscription => subscription.AlertRuleId);

        builder.Property(subscription => subscription.OwnerUserId);
        builder.HasIndex(subscription => subscription.OwnerUserId);
    }
}
