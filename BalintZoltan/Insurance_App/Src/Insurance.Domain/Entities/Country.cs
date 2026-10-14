namespace Insurance.Domain.Entities;

public class Country
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public Country(string name)
    {
        CheckCountryName(name);

        Id = Guid.NewGuid();
        Name = name;
    }

    private static void CheckCountryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Country name is required.");
        }
    }
}
