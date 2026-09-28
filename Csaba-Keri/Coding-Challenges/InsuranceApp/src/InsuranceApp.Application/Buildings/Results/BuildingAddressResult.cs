namespace InsuranceApp.Application.Buildings.Results;

public record BuildingAddressResult(Guid CityId, string Street, string Number)
{
    public Guid CityId { get; } = CityId;
    public string Street { get; } = Street;
    public string Number { get; } = Number;
}
