using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceApp.Infrastructure.Persistence.Configurations;

public class RiskFactorEntityConfiguration : IEntityTypeConfiguration<RiskFactorConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<RiskFactorConfigurationEntity> builder)
    {
        builder.ToTable("risk_factor_configurations", table =>
        {
            table.HasCheckConstraint(
                "ck_risk_factor_configurations_target",
                """
                (level = 'Country' AND country_id IS NOT NULL AND county_id IS NULL AND city_id IS NULL AND building_type IS NULL)
                OR (level = 'County' AND county_id IS NOT NULL AND country_id IS NULL AND city_id IS NULL AND building_type IS NULL)
                OR (level = 'City' AND city_id IS NOT NULL AND country_id IS NULL AND county_id IS NULL AND building_type IS NULL)
                OR (level = 'BuildingType' AND building_type IS NOT NULL AND country_id IS NULL AND county_id IS NULL AND city_id IS NULL)
                """
            );
            
            table.HasCheckConstraint(
                "ck_risk_factor_configurations_building_type",
                "building_type IS NULL OR building_type IN ('Residential', 'Office', 'Industrial')"
            );
            
            table.HasCheckConstraint(
                "ck_risk_factor_configurations_adjustment_percentage",
                $"adjustment_percentage BETWEEN {RiskFactorRules.MinAdjustment} AND {RiskFactorRules.MaxAdjustment}"
            );
        });
        
        builder.HasKey(entity => entity.Id);
        
        builder.Property(entity => entity.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        
        builder.Property(entity => entity.Level)
            .HasColumnName("level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(entity => entity.CountryId)
            .HasColumnName("country_id");
        
        builder.Property(entity => entity.CountyId)
            .HasColumnName("county_id");
        
        builder.Property(entity => entity.CityId)
            .HasColumnName("city_id");
        
        builder.Property(entity => entity.BuildingType)
            .HasColumnName("building_type")
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(entity => entity.AdjustmentPercentage)
            .HasColumnName("adjustment_percentage")
            .HasPrecision(RiskFactorRules.AdjustmentPrecision, RiskFactorRules.AdjustmentScale);
        
        builder.Property(entity => entity.IsActive)
            .HasColumnName("is_active");
        
        builder.HasOne<CountryEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CountryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DatabaseNames.RiskFactorCountryForeignKey);
        
        builder.HasOne<CountyEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CountyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DatabaseNames.RiskFactorCountyForeignKey);

        builder.HasOne<CityEntity>()
            .WithMany()
            .HasForeignKey(entity => entity.CityId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(DatabaseNames.RiskFactorCityForeignKey);

        builder.HasIndex(entity => new { entity.IsActive, entity.BuildingType });
    }
}
