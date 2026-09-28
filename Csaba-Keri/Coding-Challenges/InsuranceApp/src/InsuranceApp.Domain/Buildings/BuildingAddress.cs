namespace InsuranceApp.Domain.Buildings;

public record BuildingAddress
{
    public const int MaxStreetLength = 200;
    public const int MaxNumberLength = 20;

    public Guid CityId { get; }
    public string Street { get; }
    public string Number { get; }

    public BuildingAddress(Guid cityId, string street, string number)
    {
        if (cityId == Guid.Empty)
        {
            throw new ArgumentException(
                "City identifier must not be empty.",
                nameof(cityId)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);

        var normalizedStreet = street.Trim();
        var normalizedNumber = number.Trim();

        if (normalizedStreet.Length > MaxStreetLength)
        {
            throw new ArgumentException(
                $"Street must not exceed {MaxStreetLength} characters.",
                nameof(street)
            );
        }

        if (normalizedNumber.Length > MaxNumberLength)
        {
            throw new ArgumentException(
                $"Street number must not exceed {MaxNumberLength} characters.",
                nameof(number)
            );
        }

        CityId = cityId;
        Street = normalizedStreet;
        Number = normalizedNumber;
    }
}
