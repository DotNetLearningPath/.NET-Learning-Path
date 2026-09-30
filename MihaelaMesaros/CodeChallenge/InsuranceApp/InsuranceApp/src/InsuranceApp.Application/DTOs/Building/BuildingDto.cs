namespace InsuranceApp.Application.DTOs.Building;

public sealed record BuildingDto(
    Guid BuildingId,
    Guid ClientId,
    string AddressStreet,
    string AddressStreetNumber,
    Guid CityId,
    int ConstructionYear,
    Guid BuildingTypeId,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue,
    string? RiskIndicators
);