using System.Text.Json.Serialization;

namespace InsuranceApp.WebApi.Models.RiskFactors.Targets;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "level")]
[JsonDerivedType(typeof(CountryTargetResponse), nameof(RiskFactorLevelDto.Country))]
[JsonDerivedType(typeof(CountyTargetResponse), nameof(RiskFactorLevelDto.County))]
[JsonDerivedType(typeof(CityTargetResponse), nameof(RiskFactorLevelDto.City))]
[JsonDerivedType(typeof(BuildingTypeTargetResponse), nameof(RiskFactorLevelDto.BuildingType))]
public abstract record RiskTargetResponse
{
    private protected RiskTargetResponse()
    {
    }
}
