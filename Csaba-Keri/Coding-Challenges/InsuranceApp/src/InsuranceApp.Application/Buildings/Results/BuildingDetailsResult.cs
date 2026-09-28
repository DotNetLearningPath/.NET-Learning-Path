using InsuranceApp.Application.Geography.Results;

namespace InsuranceApp.Application.Buildings.Results;

public record BuildingDetailsResult(BuildingResult Building, CityGeographyResult Geography)
{
    public BuildingResult Building { get; } = Building;
    public CityGeographyResult Geography { get; } = Geography;
}
