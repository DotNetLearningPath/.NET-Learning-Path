using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BuildingConfiguration
    : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion<string>();

        builder.Property(x => x.ClientId)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CityId)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Street)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Number)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.SurfaceArea)
            .IsRequired();

        builder.Property(x => x.InsuredValue)
            .IsRequired();

        builder.HasOne<Client>()
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClientId);

        builder.HasIndex(x => x.CityId);
    }
}