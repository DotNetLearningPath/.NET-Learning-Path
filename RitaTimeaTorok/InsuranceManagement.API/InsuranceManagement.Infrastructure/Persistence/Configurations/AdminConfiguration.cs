using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.Property(admin => admin.Name)
                .HasMaxLength(200);

            builder.Property(admin => admin.Email)
                .HasMaxLength(200);

            builder.HasIndex(admin => admin.Email)
                .IsUnique();

            builder.Property(admin => admin.Role)
                .HasMaxLength(50);
        }
    }
}
