using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Commands;

public record CreateBuildingCommand(
    Guid ClientId,
    BuildingType Type,
    BuildingAddressCommand? Address,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue
) : IBuildingDetailsCommand;
