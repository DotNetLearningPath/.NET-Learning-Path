namespace InsuranceApp.Application.DTOs.Building;

public sealed record UpdateBuildingDto(
    Guid BuildingTypeId,
    string AddressStreet,
    string AddressStreetNumber,
    Guid CityId,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue,
    string? RiskIndicators
) : IBuildingDetailsDto;

