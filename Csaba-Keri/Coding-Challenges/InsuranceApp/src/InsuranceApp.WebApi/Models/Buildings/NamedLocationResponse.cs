namespace InsuranceApp.WebApi.Models.Buildings;

public record NamedLocationResponse(Guid Id, string Name)
{
    public Guid Id { get; } = Id;

    public string Name { get; } = Name;
}
