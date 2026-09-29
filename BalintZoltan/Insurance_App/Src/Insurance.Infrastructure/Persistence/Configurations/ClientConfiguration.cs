using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration
    : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion<string>();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IdentificationNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(x => x.IdentificationNumber)
            .IsUnique();

        builder.Property(x => x.Email)
            .HasMaxLength(254);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(500);
    }
}