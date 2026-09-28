namespace InsuranceApp.Domain.Geography;

public class County
{
    public const int MaxNameLength = 100;

    public Guid Id { get; }
    public string Name { get; }
    public Guid CountryId { get; }

    public County(Guid id, string name, Guid countryId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "County identifier must not be empty.",
                nameof(id)
            );
        }

        if (countryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Country identifier must not be empty.",
                nameof(countryId)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"County name must not exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        Id = id;
        Name = normalizedName;
        CountryId = countryId;
    }
}
