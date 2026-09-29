using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Seed;

public sealed class GeographySeeder
{
    private readonly SeedDataOptions _options;

    public GeographySeeder(IOptions<SeedDataOptions> options)
    {
        _options = options.Value;
    }

    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        var filePath = SeedFilePath.Get(
            contentRootPath, _options.BasePath, _options.GeographyFile);

        await using var stream = File.OpenRead(filePath);

        var geographyData = await JsonSerializer.DeserializeAsync<
            Dictionary<string, Dictionary<string, string>>>(
                stream,
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.GeographyFile} file is empty or invalid.");

        var countries = await dbContext.Countries
            .ToListAsync(cancellationToken);
        var counties = await dbContext.Counties
            .ToListAsync(cancellationToken);
        var cities = await dbContext.Cities
            .ToListAsync(cancellationToken);

        var country = countries.FirstOrDefault(existingCountry =>
            string.Equals(
                SeedText.Normalize(existingCountry.Name),
                "Romania",
                StringComparison.OrdinalIgnoreCase));

        if (country is null)
        {
            country = new Country("Romania");
            dbContext.Countries.Add(country);
            countries.Add(country);
        }

        foreach (var countyData in geographyData)
        {
            var county = counties.FirstOrDefault(existingCounty =>
                existingCounty.CountryId == country.Id
                && string.Equals(
                    SeedText.Normalize(existingCounty.Name),
                    SeedText.Normalize(countyData.Key),
                    StringComparison.OrdinalIgnoreCase));

            if (county is null)
            {
                county = new County(country.Id, countyData.Key);
                dbContext.Counties.Add(county);
                counties.Add(county);
            }

            foreach (var cityData in countyData.Value)
            {
                var cityExists = cities.Any(existingCity =>
                    existingCity.CountyId == county.Id
                    && string.Equals(
                        SeedText.Normalize(existingCity.Name),
                        SeedText.Normalize(cityData.Key),
                        StringComparison.OrdinalIgnoreCase));

                if (!cityExists)
                {
                    var city = new City(
                        county.Id,
                        cityData.Key,
                        cityData.Value);

                    dbContext.Cities.Add(city);
                    cities.Add(city);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
