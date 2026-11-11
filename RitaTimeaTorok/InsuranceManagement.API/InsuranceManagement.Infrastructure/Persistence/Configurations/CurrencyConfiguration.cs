using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.Property(currency => currency.Code)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(currency => currency.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(currency => currency.ExchangeRateToBase)
                .HasPrecision(18, 8);

            builder.HasIndex(currency => currency.Code)
                .IsUnique();
        }
    }
}
