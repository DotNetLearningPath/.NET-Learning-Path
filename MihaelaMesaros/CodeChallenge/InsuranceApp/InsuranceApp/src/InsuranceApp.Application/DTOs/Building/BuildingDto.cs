using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Building;

public sealed record BuildingDto(
    int BuildingId,
    int ClientId,
    string AddressStreet,
    string AddressStreetNumber,
    int CityId,
    int ConstructionYear,
    BuildingType BuildingType,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue,
    string? RiskIndicators
);