namespace Insurance.Domain.Entities;

public class City
{
    public Guid Id { get; private set; }
    public Guid CountyId { get; private set; }
    public string Name { get; private set; }
    public string PostalCode { get; private set; }

    public City(
                Guid countyId,
                string name,
                string postalCode)
    {
        CheckCityCountyId(countyId);
        CheckCityName(name);
        CheckCityPostalCode(postalCode);

        Id = Guid.NewGuid();
        CountyId = countyId;
        Name = name;
        PostalCode = postalCode;
    }

    private static void CheckCityName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("City name is required.");
        }
    }

    private static void CheckCityPostalCode(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Postal code is required.");
        }
    }

    private static void CheckCityCountyId(Guid countyId)
    {
        if (countyId == Guid.Empty)
        {
            throw new ArgumentException("County Id is required.");
        }
    }
}
