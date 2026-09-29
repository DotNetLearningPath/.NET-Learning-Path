
namespace InsuranceApp.Domain.Entities;

public sealed class Country
{
    public Guid CountryId { get; set; }
    public required string Name { get; set; }
}
