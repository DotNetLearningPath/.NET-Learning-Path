namespace InsuranceApp.Application.Geography.Results;

public record CountyResult(Guid Id, string Name, Guid CountryId)
{
    public Guid Id { get; } = Id;
    public string Name { get; } = Name;
    public Guid CountryId { get; } = CountryId;
}
