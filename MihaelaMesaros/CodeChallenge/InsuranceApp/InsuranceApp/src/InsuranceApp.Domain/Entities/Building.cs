using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Domain.Entities;

public sealed class Building
{
    public Guid BuildingId { get; set; }
    public Guid ClientId { get; set; }
    public string AddressStreet { get; set; } = null!;
    public string AddressStreetNumber { get; set; } = null!;
    public Guid CityId { get; set; }
    public int ConstructionYear { get; set; }
    public Guid BuildingTypeId { get; set; }
    public int NumberOfFloors { get; set; }
    public decimal SurfaceArea { get; set; }
    public decimal InsuredValue { get; set; }
    public string? RiskIndicators { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public Client Client { get; set; } = null!;
    public City City { get; set; } = null!;
    public BuildingType BuildingType { get; set; } = null!;

}
