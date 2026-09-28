namespace InsuranceApp.WebApi.Models.Buildings;

public record BuildingAddressResponse(Guid CityId, string Street, string Number)
{
    public Guid CityId { get; } = CityId;
    
    public string Street { get; } = Street;
    
    public string Number { get; } = Number;
}
