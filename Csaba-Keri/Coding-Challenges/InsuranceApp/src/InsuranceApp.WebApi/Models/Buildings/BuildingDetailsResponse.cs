namespace InsuranceApp.WebApi.Models.Buildings;

public record BuildingDetailsResponse(
    Guid Id,
    Guid ClientId,
    BuildingTypeDto Type,
    BuildingAddressResponse Address,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue,
    BuildingGeographyResponse Geography
)
{
    public Guid Id { get; } = Id;

    public Guid ClientId { get; } = ClientId;

    public BuildingTypeDto Type { get; } = Type;

    public BuildingAddressResponse Address { get; } = Address;

    public int ConstructionYear { get; } = ConstructionYear;

    public int NumberOfFloors { get; } = NumberOfFloors;

    public decimal SurfaceArea { get; } = SurfaceArea;

    public decimal InsuredValue { get; } = InsuredValue;

    public BuildingGeographyResponse Geography { get; } = Geography;
}
