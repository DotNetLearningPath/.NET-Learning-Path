// File: src/InsuranceApp.Infrastructure/Persistence/Configurations/BuildingTypeConfiguration.cs

using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

internal sealed class BuildingTypeConfiguration : IEntityTypeConfiguration<BuildingType>
{
    public void Configure(EntityTypeBuilder<BuildingType> builder)
    {
        builder.ToTable("BuildingTypes");

        builder.HasKey(x => x.BuildingTypeId);
        builder.Property(x => x.BuildingTypeId).ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}