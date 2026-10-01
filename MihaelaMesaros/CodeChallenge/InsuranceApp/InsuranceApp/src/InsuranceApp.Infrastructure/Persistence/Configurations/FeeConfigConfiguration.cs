using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class FeeConfigConfiguration : IEntityTypeConfiguration<FeeConfig>
{
    public void Configure(EntityTypeBuilder<FeeConfig> builder)
    {
        builder.ToTable("FeeConfigs");

        builder.HasKey(x => x.FeeConfigId);

        builder.Property(x => x.Name)
            .HasMaxLength(FeeConfigConstraints.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.FeeType)
            .IsRequired();

        builder.Property(x => x.Percentage)
            .HasPrecision(
                FeeConfigConstraints.PercentagePrecision,
                FeeConfigConstraints.PercentageScale)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}