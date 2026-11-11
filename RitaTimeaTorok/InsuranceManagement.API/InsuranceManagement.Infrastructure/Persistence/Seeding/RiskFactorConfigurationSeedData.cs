using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence.Seeding
{
    public static class RiskFactorConfigurationSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RiskFactorConfiguration>().HasData(
                new
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa1"),
                    Level = RiskFactorLevel.BuildingType,
                    CountryId = (Guid?)null,
                    CountyId = (Guid?)null,
                    CityId = (Guid?)null,
                    BuildingType = (BuildingType?)BuildingType.Residential,
                    AdjustmentPercentage = 5m,
                    IsActive = true
                },
                new
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaa2"),
                    Level = RiskFactorLevel.BuildingType,
                    CountryId = (Guid?)null,
                    CountyId = (Guid?)null,
                    CityId = (Guid?)null,
                    BuildingType = (BuildingType?)BuildingType.Industrial,
                    AdjustmentPercentage = 10m,
                    IsActive = true
                });
        }
    }
}
