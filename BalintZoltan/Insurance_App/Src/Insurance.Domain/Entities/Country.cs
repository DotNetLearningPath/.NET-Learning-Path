namespace Domain.Entities;

public class Country
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private readonly List<County> _counties = new();

    public IReadOnlyCollection<County> Counties => _counties;
    private static void CheckCountryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Country name is required.");
    }

    public Country(string name)
    {
        CheckCountryName(name);

        Id = Guid.NewGuid();
        Name = name;
    }
}