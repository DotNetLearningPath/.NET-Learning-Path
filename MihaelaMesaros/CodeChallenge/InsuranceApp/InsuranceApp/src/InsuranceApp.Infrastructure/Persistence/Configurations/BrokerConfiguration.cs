using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers");

        builder.HasKey(x => x.BrokerId);

        builder.Property(x => x.BrokerCode)
            .HasMaxLength(BrokerConstraints.BrokerCodeMaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(x => x.BrokerCode)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(BrokerConstraints.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(BrokerConstraints.EmailMaxLength)
            .IsUnicode(false);

        builder.Property(x => x.Phone)
            .HasMaxLength(BrokerConstraints.PhoneMaxLength)
            .IsUnicode(false);

        builder.Property(x => x.CommissionPercentage)
            .HasPrecision(5, 2);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt);
    }
}