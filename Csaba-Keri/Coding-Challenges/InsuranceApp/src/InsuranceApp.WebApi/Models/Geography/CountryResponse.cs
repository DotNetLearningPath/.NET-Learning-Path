namespace InsuranceApp.WebApi.Models.Geography;

public record CountryResponse(Guid Id, string Name)
{
    public Guid Id { get; } = Id;

    public string Name { get; } = Name;
}
