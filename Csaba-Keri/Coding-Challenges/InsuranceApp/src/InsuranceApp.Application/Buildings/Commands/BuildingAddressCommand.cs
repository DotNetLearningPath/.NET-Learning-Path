namespace InsuranceApp.Application.Buildings.Commands;

public record BuildingAddressCommand(
    Guid CityId,
    string? Street,
    string? Number
);
