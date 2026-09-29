using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.DTOs.Building;

public interface IBuildingDetailsDto
{
    BuildingType BuildingType { get; }
    string AddressStreet { get; }
    string AddressStreetNumber { get; }
    int CityId { get; }
    int ConstructionYear { get; }
    int NumberOfFloors { get; }
    decimal SurfaceArea { get; }
    decimal InsuredValue { get; }
    string? RiskIndicators { get; }
}