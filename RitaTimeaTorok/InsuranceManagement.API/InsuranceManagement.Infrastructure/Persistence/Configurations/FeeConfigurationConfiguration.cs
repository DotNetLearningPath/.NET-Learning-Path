using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class FeeConfigurationConfiguration : IEntityTypeConfiguration<FeeConfiguration>
    {
        public void Configure(EntityTypeBuilder<FeeConfiguration> builder)
        {
            builder.Property(configuration => configuration.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(configuration => configuration.Type)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(configuration => configuration.Percentage)
                .HasPrecision(18, 4);
        }
    }
}
