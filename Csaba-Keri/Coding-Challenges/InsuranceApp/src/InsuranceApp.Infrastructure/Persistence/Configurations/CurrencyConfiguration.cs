using InsuranceApp.Domain.Currencies;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<CurrencyEntity>
{
    public void Configure(EntityTypeBuilder<CurrencyEntity> builder)
    {
        builder.ToTable("currencies", table =>
        {
            table.HasCheckConstraint("ck_currencies_exchange_rate_to_base", "exchange_rate_to_base > 0");
        });
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Code)
            .HasColumnName("code")
            .HasMaxLength(CurrencyRules.CodeLength)
            .IsRequired()
            .UseCollation("C");
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(CurrencyRules.MaxNameLength)
            .IsRequired();
        
        builder.Property(entity => entity.ExchangeRateToBase)
            .HasColumnName("exchange_rate_to_base")
            .HasPrecision(CurrencyRules.ExchangeRatePrecision, CurrencyRules.ExchangeRateScale);
        
        builder.Property(entity => entity.IsActive)
            .HasColumnName("is_active");
        
        builder.HasIndex(entity => entity.Code)
            .IsUnique()
            .HasDatabaseName(DatabaseNames.CurrencyCodeIndex);
    }
}
