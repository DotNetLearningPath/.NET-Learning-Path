using InsuranceApp.Domain.Clients;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<ClientEntity>
{
    public void Configure(EntityTypeBuilder<ClientEntity> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.IdentificationNumber)
            .HasColumnName("identification_number")
            .HasMaxLength(Client.MaxIdentificationNumberLength)
            .IsRequired()
            .UseCollation("C");

        builder.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(Client.MaxNameLength)
            .IsRequired();

        builder.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasMaxLength(Client.MaxEmailLength)
            .IsRequired();

        builder.Property(entity => entity.Phone)
            .HasColumnName("phone")
            .HasMaxLength(Client.MaxPhoneLength)
            .IsRequired();

        builder.Property(entity => entity.PrimaryAddress)
            .HasColumnName("primary_address")
            .HasMaxLength(Client.MaxPrimaryAddressLength);
        
        builder.HasIndex(entity => entity.IdentificationNumber)
            .IsUnique()
            .HasDatabaseName(DatabaseNames.ClientIdentifierIndex);

        builder.HasIndex(entity => new { entity.Name, entity.Id });
        
        builder.ToTable("clients", table =>
            table.HasCheckConstraint("ck_clients_type","type IN ('Individual', 'Company')")
        );
    }
}
