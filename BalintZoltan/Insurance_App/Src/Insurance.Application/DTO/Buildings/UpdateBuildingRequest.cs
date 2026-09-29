using Domain.Enums;

namespace Application.DTO.Buildings;

public class UpdateBuildingRequest
{
    public Guid CityId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public int ConstructionYear { get; set; }
    public BuildingType Type { get; set; }
    public int NumberOfFloors { get; set; }
    public decimal SurfaceArea { get; set; }
    public decimal InsuredValue { get; set; }
    public bool IsFloodRiskZone { get; set; }
    public bool IsEarthquakeRiskZone { get; set; }
}