using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Insurance.Infrastructure.Configurations
{
    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.HasKey(building => building.Id);

            builder.Property(building => building.ConstructionYear)
                .IsRequired();

            builder.Property(building => building.Street)
                .IsRequired();

            builder.Property(building => building.StreetNumber)
                .IsRequired();

            builder.Property(building => building.SurfaceArea).HasPrecision(18, 2);

            builder.Property(building => building.InsuredValue).HasPrecision(18, 2);

            builder.ToTable(table =>
                table.HasCheckConstraint("CK_Building_SurfaceArea", "SurfaceArea > 0"));

            builder.ToTable(table =>
                table.HasCheckConstraint("CK_Building_InsuredValue", "InsuredValue > 0"));

            builder.HasOne(building => building.City)
                .WithMany(city => city.Buildings)
                .HasForeignKey(building => building.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
