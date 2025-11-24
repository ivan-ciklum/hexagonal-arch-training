using ClubExample.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubExample.Adapter.PostgreSQL.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate IDs in the domain

        builder.Property(m => m.ClubId)
            .IsRequired();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.SubscriptionId)
            .IsRequired();

        // Index for the business rule: unique name per club
        builder.HasIndex(m => new { m.Name, m.ClubId })
            .IsUnique()
            .HasDatabaseName("IX_Members_Name_ClubId");
    }
}
