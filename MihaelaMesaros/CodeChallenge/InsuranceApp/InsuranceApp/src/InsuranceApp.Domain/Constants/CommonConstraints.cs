namespace InsuranceApp.Domain.Constants;

public static class CommonConstraints
{
    public const int MinPageNumber = 1;
    public const int MaxPageNumber = 1000;

    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public const int SurfaceDecimalPlaces = 2;

    public const int PriceDecimalPlaces = 2;

    public const MidpointRounding MidpointRoundingMode = MidpointRounding.AwayFromZero;
}
