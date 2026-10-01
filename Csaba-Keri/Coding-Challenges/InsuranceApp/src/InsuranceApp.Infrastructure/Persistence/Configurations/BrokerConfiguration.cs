using InsuranceApp.Domain.Brokers;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal class BrokerConfiguration : IEntityTypeConfiguration<BrokerEntity>
{
    public void Configure(EntityTypeBuilder<BrokerEntity> builder)
    {
        builder.ToTable("brokers", table =>
            table.HasCheckConstraint("ck_brokers_status", "status IN ('Active', 'Inactive')")
        );
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Code)
            .HasColumnName("code")
            .HasMaxLength(Broker.MaxCodeLength)
            .IsRequired()
            .UseCollation("C");
        
        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(Broker.MaxNameLength)
            .IsRequired();
        
        builder.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasMaxLength(Broker.MaxEmailLength)
            .IsRequired();
        
        builder.Property(entity => entity.Phone)
            .HasColumnName("phone")
            .HasMaxLength(Broker.MaxPhoneLength)
            .IsRequired();
        
        builder.Property(entity => entity.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasIndex(entity => entity.Code)
            .IsUnique()
            .HasDatabaseName(DatabaseNames.BrokerCodeIndex);
        
        builder.HasIndex(entity => new { entity.Name, entity.Id });
    }
}
