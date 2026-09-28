namespace InsuranceApp.Domain.Buildings;

public class Building
{
    public const int MinConstructionYear = 1;
    public const int MaxConstructionYear = 9999;
    public const int DecimalPlaces = 2;
    public const decimal MaxSurfaceArea = 9_999_999_999.99m;
    public const decimal MaxInsuredValue = 9_999_999_999_999_999.99m;

    public Guid Id { get; }
    public Guid ClientId { get; }

    public BuildingType Type { get; private set; }
    public BuildingAddress Address { get; private set; }
    public int ConstructionYear { get; private set; }
    public int NumberOfFloors { get; private set; }
    public decimal SurfaceArea { get; private set; }
    public decimal InsuredValue { get; private set; }

    public Building(
        Guid id,
        Guid clientId,
        BuildingType type,
        BuildingAddress address,
        int constructionYear,
        int numberOfFloors,
        decimal surfaceArea,
        decimal insuredValue
    )
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Building identifier must not be empty.",
                nameof(id)
            );
        }

        if (clientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Client identifier must not be empty.",
                nameof(clientId)
            );
        }

        ValidateDetails(type, address, constructionYear, numberOfFloors, surfaceArea, insuredValue);

        Id = id;
        ClientId = clientId;
        Type = type;
        Address = address;
        ConstructionYear = constructionYear;
        NumberOfFloors = numberOfFloors;
        SurfaceArea = surfaceArea;
        InsuredValue = insuredValue;
    }

    public void UpdateDetails(
        BuildingType type,
        BuildingAddress address,
        int constructionYear,
        int numberOfFloors,
        decimal surfaceArea,
        decimal insuredValue
    )
    {
        ValidateDetails(type, address, constructionYear, numberOfFloors, surfaceArea, insuredValue);

        Type = type;
        Address = address;
        ConstructionYear = constructionYear;
        NumberOfFloors = numberOfFloors;
        SurfaceArea = surfaceArea;
        InsuredValue = insuredValue;
    }

    private static void ValidateDetails(
        BuildingType type,
        BuildingAddress address,
        int constructionYear,
        int numberOfFloors,
        decimal surfaceArea,
        decimal insuredValue
    )
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Building type is invalid."
            );
        }

        ArgumentNullException.ThrowIfNull(address);

        if (constructionYear is < MinConstructionYear or > MaxConstructionYear)
        {
            throw new ArgumentOutOfRangeException(
                nameof(constructionYear),
                constructionYear,
                $"Construction year must be between {MinConstructionYear} and {MaxConstructionYear}."
            );
        }

        if (numberOfFloors < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfFloors),
                numberOfFloors,
                "A building must have at least one floor, including the ground floor."
            );
        }

        ValidatePositiveDecimal(surfaceArea, MaxSurfaceArea, nameof(surfaceArea));
        ValidatePositiveDecimal(insuredValue, MaxInsuredValue, nameof(insuredValue));
    }

    private static void ValidatePositiveDecimal(decimal value, decimal maximum, string parameterName)
    {
        if (value <= 0 || value > maximum)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Value must be greater than zero and at most {maximum}."
            );
        }

        if (decimal.Round(value, DecimalPlaces) != value)
        {
            throw new ArgumentException(
                $"Value must have at most {DecimalPlaces} decimal places.",
                parameterName
            );
        }
    }
}
