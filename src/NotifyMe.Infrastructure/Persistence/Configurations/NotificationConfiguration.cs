using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotifyMe.Domain.Notifications;

namespace NotifyMe.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.AlertRuleId).IsRequired();
        builder.Property(notification => notification.ChannelConfigId).IsRequired();
        builder.Property(notification => notification.NormalizedEventId).IsRequired();

        builder.Property(notification => notification.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(notification => notification.SentAt);
        builder.Property(notification => notification.Error).HasMaxLength(2000);

        builder.HasIndex(notification => notification.Status);
        builder.HasIndex(notification => notification.AlertRuleId);
    }
}
