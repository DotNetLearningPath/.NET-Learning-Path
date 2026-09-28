using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Commands;

public interface IBuildingDetailsCommand
{
    BuildingType Type { get; }
    BuildingAddressCommand? Address { get; }
    int ConstructionYear { get; }
    int NumberOfFloors { get; }
    decimal SurfaceArea { get; }
    decimal InsuredValue { get; }
}
