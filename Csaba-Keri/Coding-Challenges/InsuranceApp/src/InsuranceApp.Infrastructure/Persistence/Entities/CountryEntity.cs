namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class CountryEntity(Guid id, string name)
{
    public Guid Id { get; private set; } = id;
    public string Name { get; private set; } = name;
}
