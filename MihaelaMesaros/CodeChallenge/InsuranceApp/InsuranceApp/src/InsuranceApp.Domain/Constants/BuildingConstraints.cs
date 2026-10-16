namespace InsuranceApp.Domain.Constants;

public static class BuildingConstraints
{
    public const int AddressStreetMaxLength = 200;
    public const int AddressStreetNumberMaxLength = 20;
    public const int RiskIndicatorsMaxLength = 1000;

    public const int MinConstructionYear = 1800;

    public const int MinNumberOfFloors = 0;
    public const int MaxNumberOfFloors = 200;

    public const decimal MinSurfaceArea = 0.01m;
    public const decimal MaxSurfaceArea = 999_999_999.99m;
    public const int SurfaceDecimalPlaces = 2;

    public const decimal MinInsuredValue = 0.01m;
    public const decimal MaxInsuredValue = 999_999_999_999.99m;
}
