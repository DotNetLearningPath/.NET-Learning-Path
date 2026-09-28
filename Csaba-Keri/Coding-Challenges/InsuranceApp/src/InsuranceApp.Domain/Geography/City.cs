namespace InsuranceApp.Domain.Geography;

public class City
{
    public const int MaxNameLength = 100;

    public Guid Id { get; }
    public string Name { get; }
    public Guid CountyId { get; }

    public City(Guid id, string name, Guid countyId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "City identifier must not be empty.",
                nameof(id)
            );
        }

        if (countyId == Guid.Empty)
        {
            throw new ArgumentException(
                "County identifier must not be empty.",
                nameof(countyId)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"City name must not exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        Id = id;
        Name = normalizedName;
        CountyId = countyId;
    }
}
