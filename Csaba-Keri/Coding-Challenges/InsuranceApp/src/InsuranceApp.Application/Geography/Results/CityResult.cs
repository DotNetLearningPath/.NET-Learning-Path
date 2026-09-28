namespace InsuranceApp.Application.Geography.Results;

public record CityResult(Guid Id, string Name, Guid CountyId)
{
    public Guid Id { get; } = Id;
    public string Name { get; } = Name;
    public Guid CountyId { get; } = CountyId;
}
