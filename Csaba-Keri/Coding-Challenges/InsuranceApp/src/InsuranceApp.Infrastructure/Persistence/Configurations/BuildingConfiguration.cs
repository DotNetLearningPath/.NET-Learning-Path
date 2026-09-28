using InsuranceApp.Domain.Buildings;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<BuildingEntity>
{
    public void Configure(EntityTypeBuilder<BuildingEntity> builder)
    {
        builder.ToTable("buildings");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.ClientId)
            .HasColumnName("client_id");

        builder.Property(entity => entity.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(entity => entity.CityId)
            .HasColumnName("city_id");

        builder.Property(entity => entity.Street)
            .HasColumnName("street")
            .HasMaxLength(BuildingAddress.MaxStreetLength)
            .IsRequired();
        
        builder.Property(entity => entity.Number)
            .HasColumnName("number")
            .HasMaxLength(BuildingAddress.MaxNumberLength)
            .IsRequired();

        builder.Property(entity => entity.ConstructionYear)
            .HasColumnName("construction_year");
        
        builder.Property(entity => entity.NumberOfFloors)
            .HasColumnName("number_of_floors");
        
        builder.Property(entity => entity.SurfaceArea)
            .HasColumnName("surface_area")
            .HasPrecision(12, Building.DecimalPlaces);
        
        builder.Property(entity => entity.InsuredValue)
            .HasColumnName("insured_value")
            .HasPrecision(18, Building.DecimalPlaces);
        
        builder.HasIndex(entity => new { entity.ClientId, entity.Id });
        
        builder.HasOne<ClientEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DatabaseNames.BuildingClientForeignKey);
        
        builder.HasOne<CityEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DatabaseNames.BuildingCityForeignKey);
        
        builder.ToTable("buildings", table =>
        {
            table.HasCheckConstraint("ck_buildings_type", "type IN ('Residential', 'Office', 'Industrial')");
            table.HasCheckConstraint("ck_buildings_construction_year", $"construction_year BETWEEN {Building.MinConstructionYear} AND {Building.MaxConstructionYear}");
            table.HasCheckConstraint("ck_buildings_number_of_floors", "number_of_floors >= 1");
            table.HasCheckConstraint("ck_buildings_surface_area", "surface_area > 0");
            table.HasCheckConstraint("ck_buildings_insured_value", "insured_value > 0");
        });
    }
}
