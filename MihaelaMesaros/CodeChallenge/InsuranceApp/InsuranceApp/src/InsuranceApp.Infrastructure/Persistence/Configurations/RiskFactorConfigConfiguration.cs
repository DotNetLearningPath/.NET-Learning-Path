using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .HasPrecision(
                RiskFactorConfigConstraints.AdjustmentPercentagePrecision,
                RiskFactorConfigConstraints.AdjustmentPercentageScale)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ModifiedAt);

        builder.HasIndex(x => new
            {
                x.Level,
                x.ReferenceId
            })
            .IsUnique();
    }
}