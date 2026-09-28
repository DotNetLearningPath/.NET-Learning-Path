using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RiskFactorConfigurationConfiguration : IEntityTypeConfiguration<RiskFactorConfiguration>
{
    public void Configure(EntityTypeBuilder<RiskFactorConfiguration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion<string>();
        builder.Property(x => x.Level).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Reference)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.AdjustmentPercentage).HasPrecision(8, 4).IsRequired();
        builder.HasIndex(x => new { x.Level, x.Reference }).IsUnique();
    }
}
