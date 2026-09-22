using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotifyMe.Domain.Users;

namespace NotifyMe.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(user => user.CreatedAt).IsRequired();
    }
}
