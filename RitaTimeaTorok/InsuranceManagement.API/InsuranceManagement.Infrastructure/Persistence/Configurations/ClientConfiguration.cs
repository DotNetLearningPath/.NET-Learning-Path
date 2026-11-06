using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(c => c.Name)
                .HasMaxLength(200);

            builder.Property(c => c.IdentificationNumber)
                .HasMaxLength(20);

            builder.HasIndex(c => c.IdentificationNumber)
                .IsUnique();

            builder.Property(c => c.Email)
                .HasMaxLength(200);

            builder.Property(c => c.PhoneNumber)
                .HasMaxLength(30);

            builder.Property(c => c.PrimaryAddress)
                .HasMaxLength(250);
        }
    }
}
