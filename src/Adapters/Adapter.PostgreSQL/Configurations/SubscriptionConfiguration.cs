using ClubExample.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubExample.Adapter.PostgreSQL.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(s => s.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.StartDate)
            .IsRequired();

        builder.Property(s => s.EndDate)
            .IsRequired();
    }
}
