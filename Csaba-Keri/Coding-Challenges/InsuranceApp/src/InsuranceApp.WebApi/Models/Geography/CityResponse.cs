namespace InsuranceApp.WebApi.Models.Geography;

public record CityResponse(Guid Id, string Name, Guid CountyId)
{
    public Guid Id { get; } = Id;

    public string Name { get; } = Name;

    public Guid CountyId { get; } = CountyId;
}
