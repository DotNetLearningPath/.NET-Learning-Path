using InsuranceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.Property(b => b.InsuredValue)
            .HasPrecision(18, 3);

        builder.Property(b => b.SurfaceArea)
            .HasPrecision(18, 3);
    }
}