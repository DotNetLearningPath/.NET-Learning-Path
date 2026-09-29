using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class RiskFactorConfigConfiguration : IEntityTypeConfiguration<RiskFactorConfig>
{
    public void Configure(EntityTypeBuilder<RiskFactorConfig> builder)
    {
        builder.ToTable("RiskFactorConfigs");

        builder.HasKey(x => x.RiskFactorConfigId);

        builder.Property(x => x.Level)
            .IsRequired();

        builder.Property(x => x.ReferenceId)
            .IsRequired();

        builder.Property(x => x.AdjustmentPercentage)
            .HasPrecision(7, 4)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Level,
            x.ReferenceId
        });
    }
}