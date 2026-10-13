using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class PolicyAdjustmentEntityConfiguration : IEntityTypeConfiguration<PolicyAdjustmentEntity>
{
    public void Configure(EntityTypeBuilder<PolicyAdjustmentEntity> builder)
    {
        builder.ToTable("policy_adjustments", table =>
        {
            table.HasCheckConstraint("ck_policy_adjustments_source", "source IN ('Fee', 'RiskFactor')");
            
            table.HasCheckConstraint(
                "ck_policy_adjustments_percentage",
                $"""
                (source = 'Fee' AND percentage BETWEEN {FeeRules.MinPercentage} AND {FeeRules.MaxPercentage})
                OR (source = 'RiskFactor' AND percentage BETWEEN {RiskFactorRules.MinAdjustment} AND {RiskFactorRules.MaxAdjustment})
                """
            );
        });
        
        builder.HasKey(entity => new { entity.PolicyId, entity.Source, entity.ConfigurationId });
        
        builder.Property(entity => entity.PolicyId)
            .HasColumnName("policy_id");
        
        builder.Property(entity => entity.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.ConfigurationId)
            .HasColumnName("configuration_id");
        
        builder.Property(entity => entity.Percentage)
            .HasColumnName("percentage");
    }
}
