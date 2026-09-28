namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class CountyEntity(Guid id, string name, Guid countryId)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
    public Guid CountryId { get; private set; } = countryId;
}
