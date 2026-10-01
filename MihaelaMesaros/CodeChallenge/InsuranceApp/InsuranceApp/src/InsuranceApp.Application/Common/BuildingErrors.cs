using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class BuildingErrors
{
    public static readonly Error InvalidBuildingId = new(
        "Building.InvalidBuildingId",
        "Building ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error InvalidClientId = new(
        "Building.InvalidClientId",
        "Client ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error InvalidCityId = new(
        "Building.InvalidCityId",
        "City ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error AddressStreetRequired = new(
        "Building.AddressStreetRequired",
        "Street address is required.",
        ErrorType.Validation);

    public static readonly Error InvalidAddressStreetLength = new(
        "Building.InvalidAddressStreetLength",
        $"Street address must not exceed {BuildingConstraints.AddressStreetMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error AddressStreetNumberRequired = new(
        "Building.AddressStreetNumberRequired",
        "Street number is required.",
        ErrorType.Validation);

    public static readonly Error InvalidAddressStreetNumberLength = new(
        "Building.InvalidAddressStreetNumberLength",
        $"Street number must not exceed {BuildingConstraints.AddressStreetNumberMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidConstructionYear = new(
        "Building.InvalidConstructionYear",
        $"Construction year must be between {BuildingConstraints.MinConstructionYear} and current year.",
        ErrorType.Validation);

    public static readonly Error InvalidBuildingType = new(
        "Building.InvalidBuildingType",
        "Building type is invalid.",
        ErrorType.Validation);

    public static readonly Error InvalidNumberOfFloors = new(
        "Building.InvalidNumberOfFloors",
        $"Number of floors must be in range {BuildingConstraints.MinNumberOfFloors} - {BuildingConstraints.MaxNumberOfFloors}.",
        ErrorType.Validation);

    public static readonly Error InvalidSurfaceArea = new(
        "Building.InvalidSurfaceArea",
        $"Surface area must be in range {BuildingConstraints.MinSurfaceArea} - {BuildingConstraints.MaxSurfaceArea}.",
        ErrorType.Validation);
    public static readonly Error InvalidSurfaceAreaScale = new(
        "Building.InvalidSurfaceAreaScale",
        $"Surface area must have a maximum of {CommonConstraints.DecimalScale} decimal places.",
        ErrorType.Validation);

    public static readonly Error InvalidInsuredValue = new(
        "Building.InvalidInsuredValue",
        $"Insured value must be in range {BuildingConstraints.MinInsuredValue} - {BuildingConstraints.MaxInsuredValue}.",
        ErrorType.Validation);
    public static readonly Error InvalidInsuredValueScale = new(
        "Building.InvalidInsuredValueScale",
        $"Insured value must have a maximum of {CommonConstraints.DecimalScale} decimal places.",
        ErrorType.Validation);

    public static readonly Error InvalidRiskIndicatorsLength = new(
        "Building.InvalidRiskIndicatorsLength",
        $"Risk indicators must not exceed {BuildingConstraints.RiskIndicatorsMaxLength} characters.",
        ErrorType.Validation);

    public static Error NotFound(Guid buildingId) => new(
        "Building.NotFound",
        $"Building with ID {buildingId} was not found.",
        ErrorType.NotFound);

    public static Error CityNotFound(Guid cityId) => new(
        "Building.CityNotFound",
        $"City with ID {cityId} was not found.",
        ErrorType.NotFound);

    public static Error BuildingTypeNotFound(Guid buildingTypeId) => new(
        "Building.BuildingTypeNotFound",
        $"Building type with ID {buildingTypeId} was not found.",
        ErrorType.NotFound);

}