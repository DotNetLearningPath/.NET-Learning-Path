using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Building;

public sealed record UpdateBuildingDto(
    BuildingType BuildingType,
    string AddressStreet,
    string AddressStreetNumber,
    int CityId,
    int ConstructionYear,
    int NumberOfFloors,
    decimal SurfaceArea,
    decimal InsuredValue,
    string? RiskIndicators
) : IBuildingDetailsDto;

