namespace InsuranceApp.Application.Geography.Results;

public record CountryResult(Guid Id, string Name)
{
    public Guid Id { get; } = Id;
    public string Name { get; } = Name;
}
