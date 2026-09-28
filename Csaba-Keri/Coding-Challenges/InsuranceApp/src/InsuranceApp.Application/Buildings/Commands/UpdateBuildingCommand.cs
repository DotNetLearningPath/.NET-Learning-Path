using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Commands;

public record UpdateBuildingCommand(
    Guid BuildingId,
    BuildingType Type,
    BuildingAddressCommand? Address,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue
) : IBuildingDetailsCommand;
