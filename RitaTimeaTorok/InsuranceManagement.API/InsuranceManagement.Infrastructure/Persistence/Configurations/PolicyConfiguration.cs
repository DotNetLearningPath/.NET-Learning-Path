using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.Property(policy => policy.PolicyNumber)
                .HasMaxLength(50);

            builder.HasIndex(policy => policy.PolicyNumber)
                .IsUnique();

            builder.Property(policy => policy.BasePremiumAmount)
                .HasPrecision(18, 2);

            builder.Property(policy => policy.FinalPremiumAmount)
                .HasPrecision(18, 2);

            builder.Property(policy => policy.CancellationReason)
                .HasMaxLength(500);

            builder.HasOne<Client>()
                .WithMany()
                .HasForeignKey(policy => policy.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Building>()
                .WithMany()
                .HasForeignKey(policy => policy.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Broker>()
                .WithMany()
                .HasForeignKey(policy => policy.BrokerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(policy => policy.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
