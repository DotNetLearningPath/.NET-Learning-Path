namespace InsuranceApp.Domain.Geography;

public class Country
{
    public const int MaxNameLength = 100;

    public Guid Id { get; }
    public string Name { get; }

    public Country(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Country identifier must not be empty.",
                nameof(id)
            );
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Country name must not exceed {MaxNameLength} characters.",
                nameof(name)
            );
        }

        Id = id;
        Name = normalizedName;
    }
}
