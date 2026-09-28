using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Results;

public record BuildingResult(
    Guid Id,
    Guid ClientId,
    BuildingType Type,
    BuildingAddressResult Address,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue
)
{
    public Guid Id { get; } = Id;
    public Guid ClientId { get; } = ClientId;
    public BuildingType Type { get; } = Type;
    public BuildingAddressResult Address { get; } = Address;
    public int ConstructionYear { get; } = ConstructionYear;
    public int NumberOfFloors { get; } = NumberOfFloors;
    public decimal SurfaceArea { get; } = SurfaceArea;
    public decimal InsuredValue { get; } = InsuredValue;
}
