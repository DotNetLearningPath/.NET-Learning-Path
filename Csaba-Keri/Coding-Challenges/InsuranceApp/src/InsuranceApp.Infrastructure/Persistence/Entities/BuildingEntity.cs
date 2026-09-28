using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class BuildingEntity(
    Guid id,
    Guid clientId,
    BuildingType type,
    Guid cityId,
    string street,
    string number,
    int constructionYear,
    int numberOfFloors,
    decimal surfaceArea,
    decimal insuredValue
)
{
    public Guid Id { get; private set; } = id;
    public Guid ClientId { get; private set; } = clientId;
    public BuildingType Type { get; private set; } = type;
    public Guid CityId { get; private set; } = cityId;
    public string Street { get; private set; } = street;
    public string Number { get; private set; } = number;
    public int ConstructionYear { get; private set; } = constructionYear;
    public int NumberOfFloors { get; private set; } = numberOfFloors;
    public decimal SurfaceArea { get; private set; } = surfaceArea;
    public decimal InsuredValue { get; private set; } = insuredValue;
}
