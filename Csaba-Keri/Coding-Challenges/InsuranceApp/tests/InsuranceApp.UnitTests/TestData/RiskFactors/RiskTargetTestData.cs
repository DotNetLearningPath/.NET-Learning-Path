using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;

namespace InsuranceApp.UnitTests.TestData.RiskFactors;

internal static class RiskTargetTestData
{
    public static readonly Guid DefaultCountryId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid DefaultCountyId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid DefaultCityId =
        Guid.Parse("33333333-3333-3333-3333-333333333333");

    public const BuildingType DefaultBuildingType = BuildingType.Residential;

    public static RiskTarget Create(
        RiskFactorLevel level,
        Guid? id = null,
        BuildingType buildingType = DefaultBuildingType
    )
    {
        return level switch
        {
            RiskFactorLevel.Country => CreateCountryTarget(id),
            RiskFactorLevel.County => CreateCountyTarget(id),
            RiskFactorLevel.City => CreateCityTarget(id),
            RiskFactorLevel.BuildingType => CreateBuildingTypeTarget(buildingType),

            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }

    public static CountryTarget CreateCountryTarget(Guid? countryId = null)
    {
        return new(countryId: countryId ?? DefaultCountryId);
    }

    public static CountyTarget CreateCountyTarget(Guid? countyId = null)
    {
        return new(countyId: countyId ?? DefaultCountyId);
    }

    public static CityTarget CreateCityTarget(Guid? cityId = null)
    {
        return new(cityId: cityId ?? DefaultCityId);
    }

    public static BuildingTypeTarget CreateBuildingTypeTarget(BuildingType type = DefaultBuildingType)
    {
        return new(type: type);
    }
}
