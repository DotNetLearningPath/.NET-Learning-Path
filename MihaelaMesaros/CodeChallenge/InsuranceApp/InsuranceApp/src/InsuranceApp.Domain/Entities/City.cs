
namespace InsuranceApp.Domain.Entities;

public sealed class City
{
    public int CityId { get; set; }

    public required string Name { get; set; }

    public int CountyId { get; set; }

    public County County { get; set; } = null!;
}
