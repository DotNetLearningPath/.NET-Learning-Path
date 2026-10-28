using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.Id)
            .ValueGeneratedNever();
        
        builder.Property(user => user.Email)
            .IsRequired();
        
        builder.Property(user => user.NormalizedEmail)
            .IsRequired();
        
        builder.Property(user => user.UserName)
            .IsRequired();
        
        builder.Property(user => user.NormalizedUserName)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasColumnName("is_active");
        
        builder.Property(user => user.BrokerId)
            .HasColumnName("broker_id");

        builder.HasOne<BrokerEntity>()
            .WithOne()
            .HasForeignKey<ApplicationUser>(user => user.BrokerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();
        
        builder.HasIndex(user => user.BrokerId)
            .IsUnique();
    }
}
