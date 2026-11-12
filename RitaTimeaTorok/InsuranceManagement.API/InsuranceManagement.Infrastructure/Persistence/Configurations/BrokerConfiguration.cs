using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
    {
        public void Configure(EntityTypeBuilder<Broker> builder)
        {
            builder.Property(broker => broker.BrokerCode)
                .HasMaxLength(30);

            builder.HasIndex(broker => broker.BrokerCode)
                .IsUnique();

            builder.Property(broker => broker.Name)
                .HasMaxLength(200);

            builder.Property(broker => broker.Email)
                .HasMaxLength(200);

            builder.HasIndex(broker => broker.Email)
                .IsUnique();

            builder.Property(broker => broker.PhoneNumber)
                .HasMaxLength(30);

            builder.Property(broker => broker.CommissionPercentage)
                .HasPrecision(5, 2);
        }
    }
}
