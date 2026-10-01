namespace InsuranceApp.Application.DTOs.Building;

public interface IBuildingDetailsDto
{
    Guid BuildingTypeId { get; }

    string AddressStreet { get; }

    string AddressStreetNumber { get; }

    Guid CityId { get; }

    int ConstructionYear { get; }

    int NumberOfFloors { get; }

    decimal SurfaceArea { get; }

    decimal InsuredValue { get; }

    string? RiskIndicators { get; }
}