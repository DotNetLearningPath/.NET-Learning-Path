using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuranceManagement.Infrastructure.Persistence.Configurations
{
    public class RiskFactorConfigurationConfiguration : IEntityTypeConfiguration<RiskFactorConfiguration>
    {
        public void Configure(EntityTypeBuilder<RiskFactorConfiguration> builder)
        {
            builder.Property(configuration => configuration.AdjustmentPercentage)
                .HasPrecision(18, 4);

            builder.HasOne<Country>()
                .WithMany()
                .HasForeignKey(configuration => configuration.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<County>()
                .WithMany()
                .HasForeignKey(configuration => configuration.CountyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<City>()
                .WithMany()
                .HasForeignKey(configuration => configuration.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(table => table.HasCheckConstraint(
                "CK_RiskFactorConfigurations_LevelTarget",
                "([Level] = 1 AND [CountryId] IS NOT NULL AND [CountyId] IS NULL AND [CityId] IS NULL AND [BuildingType] IS NULL) OR " +
                "([Level] = 2 AND [CountryId] IS NULL AND [CountyId] IS NOT NULL AND [CityId] IS NULL AND [BuildingType] IS NULL) OR " +
                "([Level] = 3 AND [CountryId] IS NULL AND [CountyId] IS NULL AND [CityId] IS NOT NULL AND [BuildingType] IS NULL) OR " +
                "([Level] = 4 AND [CountryId] IS NULL AND [CountyId] IS NULL AND [CityId] IS NULL AND [BuildingType] IS NOT NULL)"));
        }
    }
}
