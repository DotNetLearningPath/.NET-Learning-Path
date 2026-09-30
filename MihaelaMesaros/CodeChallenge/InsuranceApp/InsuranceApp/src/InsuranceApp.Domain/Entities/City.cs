
namespace InsuranceApp.Domain.Entities;

public sealed class City
{
    public Guid CityId { get; set; }

    public required string Name { get; set; }

    public Guid CountyId { get; set; }

    public County County { get; set; } = null!;
}
