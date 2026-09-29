using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");

        builder.HasKey(x => x.BuildingId);
        builder.Property(x => x.BuildingId).ValueGeneratedOnAdd();

        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.CityId).IsRequired();
        builder.Property(x => x.AddressStreet).HasMaxLength(BuildingConstraints.AddressStreetMaxLength).IsRequired();
        builder.Property(x => x.AddressStreetNumber).HasMaxLength(BuildingConstraints.AddressStreetNumberMaxLength).IsRequired();
        builder.Property(x => x.ConstructionYear).IsRequired();
        builder.Property(x => x.BuildingType).IsRequired();
        builder.Property(x => x.NumberOfFloors).IsRequired();
        builder.Property(x => x.SurfaceArea).HasPrecision(18, CommonConstraints.DecimalScale).IsRequired();
        builder.Property(x => x.InsuredValue).HasPrecision(18, CommonConstraints.DecimalScale).IsRequired();
        builder.Property(x => x.RiskIndicators).HasMaxLength(BuildingConstraints.RiskIndicatorsMaxLength);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ModifiedAt);

        builder.HasOne(x => x.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClientId);
    }
}