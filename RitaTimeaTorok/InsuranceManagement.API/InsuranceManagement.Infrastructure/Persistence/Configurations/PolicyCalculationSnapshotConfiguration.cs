using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class PolicyCalculationSnapshotConfiguration
        : IEntityTypeConfiguration<PolicyCalculationSnapshot>
    {
        public void Configure(EntityTypeBuilder<PolicyCalculationSnapshot> builder)
        {
            builder.Property(snapshot => snapshot.AppliedConfigurationName)
                .HasMaxLength(200);

            builder.Property(snapshot => snapshot.AppliedFeeConfigurationType)
                .HasMaxLength(50);

            builder.Property(snapshot => snapshot.AppliedRiskFactorTargetName)
                .HasMaxLength(200);

            builder.Property(snapshot => snapshot.AppliedPercentage)
                .HasPrecision(9, 4);

            builder.HasIndex(snapshot => snapshot.PolicyId);

            builder.HasOne<Policy>()
                .WithMany(policy => policy.CalculationSnapshotEntries)
                .HasForeignKey(snapshot => snapshot.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
