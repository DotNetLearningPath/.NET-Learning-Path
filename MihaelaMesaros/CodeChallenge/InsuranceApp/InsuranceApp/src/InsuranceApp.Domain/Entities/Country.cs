
namespace InsuranceApp.Domain.Entities;

public sealed class Country
{
    public int CountryId { get; set; }
    public required string Name { get; set; }
    public ICollection<County> Counties { get; set; } = [];
}
