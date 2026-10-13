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

        builder.HasIndex(entity => entity.PolicyNumber)
            .IsUnique();
    }
}
