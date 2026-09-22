using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotifyMe.Domain.Channels;

namespace NotifyMe.Infrastructure.Persistence.Configurations;

public sealed class ChannelConfigConfiguration : IEntityTypeConfiguration<ChannelConfig>
{
    public void Configure(EntityTypeBuilder<ChannelConfig> builder)
    {
        builder.ToTable("channel_configs");
        builder.HasKey(channel => channel.Id);

        builder.Property(channel => channel.ChannelType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(channel => channel.Target)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(channel => channel.IsEnabled).IsRequired();
    }
}
