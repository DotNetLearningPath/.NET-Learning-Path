using InsuranceApp.Domain.Fees;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class FeeEntityConfiguration : IEntityTypeConfiguration<FeeConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<FeeConfigurationEntity> builder)
    {
        builder.ToTable("fee_configurations", table =>
        {
            table.HasCheckConstraint(
                "ck_fee_configurations_type",
                "type IN ('BrokerCommission', 'RiskAdjustment', 'AdminFee')"
            );
            
            table.HasCheckConstraint(
                "ck_fee_configurations_percentage",
                $"percentage BETWEEN {FeeRules.MinPercentage} AND {FeeRules.MaxPercentage}"
            );
            
            table.HasCheckConstraint(
                "ck_fee_configurations_effective_period",
                "effective_to IS NULL OR effective_to >= effective_from"
            );
        });
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(FeeRules.MaxNameLength)
            .IsRequired();
        
        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        
        builder.Property(entity => entity.Percentage)
            .HasColumnName("percentage")
            .HasPrecision(FeeRules.PercentagePrecision, FeeRules.PercentageScale);
        
        builder.Property(entity => entity.EffectiveFrom)
            .HasColumnName("effective_from");
        
        builder.Property(entity => entity.EffectiveTo)
            .HasColumnName("effective_to");
        
        builder.Property(entity => entity.IsActive)
            .HasColumnName("is_active");
        
        builder.HasIndex(entity => new { entity.Name, entity.Id });
        
        builder.HasIndex(entity => new { entity.IsActive, entity.EffectiveFrom });
    }
}
