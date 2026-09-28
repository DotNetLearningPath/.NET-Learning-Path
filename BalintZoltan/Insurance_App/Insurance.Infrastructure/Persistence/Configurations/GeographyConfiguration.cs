using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CountryConfiguration
    : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(country => country.Id);

        builder.Property(country => country.Id)
            .HasConversion<string>();

        builder.Property(country => country.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(country => country.Name)
            .IsUnique();
    }
}

public sealed class CountyConfiguration
    : IEntityTypeConfiguration<County>
{
    public void Configure(EntityTypeBuilder<County> builder)
    {
        builder.HasKey(county => county.Id);

        builder.Property(county => county.Id)
            .HasConversion<string>();

        builder.Property(county => county.CountryId)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(county => county.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne<Country>()
            .WithMany(country => country.Counties)
            .HasForeignKey(county => county.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(county => new
        {
            county.CountryId,
            county.Name
        })
        .IsUnique();
    }
}

public sealed class CityConfiguration
    : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(city => city.Id);

        builder.Property(city => city.Id)
            .HasConversion<string>();

        builder.Property(city => city.CountyId)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(city => city.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(city => city.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne<County>()
            .WithMany(county => county.Cities)
            .HasForeignKey(city => city.CountyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(city => new
        {
            city.CountyId,
            city.Name,
            city.PostalCode
        })
        .IsUnique();
    }
}