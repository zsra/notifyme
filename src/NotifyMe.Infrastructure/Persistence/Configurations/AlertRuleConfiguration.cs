using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotifyMe.Domain.Alerts;

namespace NotifyMe.Infrastructure.Persistence.Configurations;

public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("alert_rules");
        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rule => rule.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rule => rule.IsEnabled).IsRequired();
        builder.Property(rule => rule.CreatedAt).IsRequired();

        builder.Property(rule => rule.OwnerUserId);
        builder.HasIndex(rule => rule.OwnerUserId);

        builder.OwnsOne(rule => rule.Criteria, criteria =>
        {
            criteria.Property(c => c.MinimumSeverity)
                .HasConversion<string>()
                .HasColumnName("minimum_severity")
                .HasMaxLength(50)
                .IsRequired();

            var keywordsConverter = new ValueConverter<IReadOnlyCollection<string>, string>(
                keywords => JsonSerializer.Serialize(keywords, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>());

            var keywordsComparer = new ValueComparer<IReadOnlyCollection<string>>(
                (left, right) => (left ?? Array.Empty<string>()).SequenceEqual(right ?? Array.Empty<string>()),
                keywords => keywords.Aggregate(0, (hash, keyword) => HashCode.Combine(hash, keyword)),
                keywords => keywords.ToList());

            criteria.Property(c => c.Keywords)
                .HasConversion(keywordsConverter, keywordsComparer)
                .HasColumnName("keywords")
                .IsRequired();
        });

        builder.Navigation(rule => rule.Criteria).IsRequired();
    }
}
