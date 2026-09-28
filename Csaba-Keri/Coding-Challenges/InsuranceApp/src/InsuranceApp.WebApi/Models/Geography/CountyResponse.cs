namespace InsuranceApp.WebApi.Models.Geography;

public record CountyResponse(Guid Id, string Name, Guid CountryId)
{
    public Guid Id { get; } = Id;

    public string Name { get; } = Name;

    public Guid CountryId { get; } = CountryId;
}
