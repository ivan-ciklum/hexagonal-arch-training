using ClubExample.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClubExample.Adapter.PostgreSQL.Configurations;

internal sealed class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
