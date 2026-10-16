using InsuranceApp.Domain.Currencies;
using InsuranceApp.Domain.Policies;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class PolicyEntityConfiguration : IEntityTypeConfiguration<PolicyEntity>
{
    public void Configure(EntityTypeBuilder<PolicyEntity> builder)
    {
        builder.ToTable("policies", table =>
        {
            table.HasCheckConstraint("ck_policies_status", "status IN ('Draft', 'Active', 'Expired', 'Cancelled')");

            table.HasCheckConstraint("ck_policies_period", "end_date >= start_date");

            table.HasCheckConstraint("ck_policies_premiums", $"base_premium > 0 AND final_premium >= {PolicyRules.MinFinalPremium}");

            table.HasCheckConstraint("ck_policies_currency_exchange_rate_to_base", "currency_exchange_rate_to_base > 0");

            table.HasCheckConstraint(
                "ck_policies_lifecycle_timestamps",
                """
                updated_at >= created_at
                AND (activated_at IS NULL OR (activated_at >= created_at AND activated_at <= updated_at AND status <> 'Draft'))
                AND (cancelled_at IS NULL OR (cancelled_at >= created_at AND cancelled_at <= updated_at
                AND (activated_at IS NULL OR cancelled_at >= activated_at)))
                AND (expired_at IS NULL OR (expired_at >= created_at AND expired_at <= updated_at
                AND (activated_at IS NULL OR expired_at >= activated_at)))
                """
            );

            table.HasCheckConstraint(
                "ck_policies_cancellation_audit",
                """
                (cancelled_at IS NULL AND cancellation_effective_date IS NULL AND cancellation_reason IS NULL)
                OR (status = 'Cancelled' AND cancelled_at IS NOT NULL AND cancellation_effective_date IS NOT NULL
                AND cancellation_reason IS NOT NULL AND length(btrim(cancellation_reason)) > 0
                AND cancellation_effective_date = (cancelled_at AT TIME ZONE 'UTC')::date
                AND cancellation_effective_date <= end_date)
                """
            );

            table.HasCheckConstraint(
                "ck_policies_expiry_audit",
                "expired_at IS NULL OR (status = 'Expired' AND end_date < (expired_at AT TIME ZONE 'UTC')::date)"
            );
        });

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(entity => entity.PolicyNumber)
            .HasColumnName("policy_number")
            .HasMaxLength(PolicyRules.MaxPolicyNumberLength)
            .IsRequired();

        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(entity => entity.ClientId)
            .HasColumnName("client_id");

        builder.Property(entity => entity.BuildingId)
            .HasColumnName("building_id");

        builder.Property(entity => entity.BrokerId)
            .HasColumnName("broker_id");

        builder.Property(entity => entity.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(entity => entity.CurrencyExchangeRateToBase)
            .HasColumnName("currency_exchange_rate_to_base")
            .HasPrecision(CurrencyRules.ExchangeRatePrecision, CurrencyRules.ExchangeRateScale);

        builder.Property(entity => entity.StartDate)
            .HasColumnName("start_date");

        builder.Property(entity => entity.EndDate)
            .HasColumnName("end_date");

        builder.Property(entity => entity.BasePremium)
            .HasColumnName("base_premium")
            .HasPrecision(PolicyRules.MoneyPrecision, PolicyRules.MoneyScale);

        builder.Property(entity => entity.FinalPremium)
            .HasColumnName("final_premium")
            .HasPrecision(PolicyRules.MoneyPrecision, PolicyRules.MoneyScale);

        builder.Property(entity => entity.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(entity => entity.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(entity => entity.ActivatedAt)
            .HasColumnName("activated_at");

        builder.Property(entity => entity.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(entity => entity.CancellationEffectiveDate)
            .HasColumnName("cancellation_effective_date");

        builder.Property(entity => entity.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(PolicyRules.MaxCancellationReasonLength);

        builder.Property(entity => entity.ExpiredAt)
            .HasColumnName("expired_at");

        builder.HasOne<ClientEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.ClientId)
            .HasConstraintName(DatabaseNames.PolicyClientForeignKey)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BuildingEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.BuildingId)
            .HasConstraintName(DatabaseNames.PolicyBuildingForeignKey)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<BrokerEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.BrokerId)
            .HasConstraintName(DatabaseNames.PolicyBrokerForeignKey)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CurrencyEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CurrencyId)
            .HasConstraintName(DatabaseNames.PolicyCurrencyForeignKey)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(entity => entity.AppliedAdjustments)
            .WithOne()
            .HasForeignKey(entity => entity.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entity => new { entity.Status, entity.EndDate });

        builder.HasIndex(entity => entity.PolicyNumber)
            .IsUnique();
    }
}
