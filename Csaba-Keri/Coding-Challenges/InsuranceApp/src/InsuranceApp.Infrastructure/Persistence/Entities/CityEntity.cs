namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class CityEntity(Guid id, string name, Guid countyId)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public Guid CountyId { get; private set; } = countyId;
}
