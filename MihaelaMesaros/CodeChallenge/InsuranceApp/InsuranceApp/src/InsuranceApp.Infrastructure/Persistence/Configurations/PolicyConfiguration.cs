using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(x => x.PolicyId);

        builder.Property(x => x.PolicyNumber)
            .HasMaxLength(PolicyConstraints.PolicyNumberMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.PolicyNumber)
            .IsUnique();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.BasePremium)
            .HasPrecision(18, CommonConstraints.DecimalScale)
            .IsRequired();

        builder.Property(x => x.FinalPremium)
            .HasPrecision(18, CommonConstraints.DecimalScale)
            .IsRequired();

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(PolicyConstraints.CancellationReasonMaxLength);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Broker)
            .WithMany()
            .HasForeignKey(x => x.BrokerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.BrokerId);
        builder.HasIndex(x => x.CurrencyId);
        builder.HasIndex(x => x.Status);
    }
}