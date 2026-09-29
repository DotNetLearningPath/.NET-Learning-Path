using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Seed;

public sealed class BuildingSeeder
{
    private readonly SeedDataOptions _options;
    private readonly ClientSeeder _clientSeeder;

    public BuildingSeeder(
        IOptions<SeedDataOptions> options,
        ClientSeeder clientSeeder)
    {
        _options = options.Value;
        _clientSeeder = clientSeeder;
    }

    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        var buildings = await LoadBuildingsAsync(
            contentRootPath,
            cancellationToken);
        var clients = await _clientSeeder.LoadAsync(
            contentRootPath,
            cancellationToken);
        var cities = await LoadCitiesAsync(
            contentRootPath,
            dbContext,
            cancellationToken);

        var clientEntities = await dbContext.Clients
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var clientIdsBySourceId = clients
            .Select((client, index) => new { SourceId = index + 1, client.Identifier })
            .Join(
                clientEntities,
                source => source.Identifier,
                client => client.IdentificationNumber,
                (source, client) => new { source.SourceId, client.Id })
            .ToDictionary(item => item.SourceId, item => item.Id);

        var existingKeys = await dbContext.Buildings
            .Select(building => new
            {
                building.ClientId,
                building.CityId,
                building.Street,
                building.Number
            })
            .ToListAsync(cancellationToken);

        foreach (var buildingData in buildings)
        {
            if (!clientIdsBySourceId.TryGetValue(
                    buildingData.ClientId,
                    out var clientId))
            {
                throw new InvalidOperationException(
                    $"Client source id '{buildingData.ClientId}' was not found.");
            }

            if (!cities.TryGetValue(
                    buildingData.CityId,
                    out var cityId))
            {
                throw new InvalidOperationException(
                    $"City source id '{buildingData.CityId}' was not found.");
            }

            var alreadyExists = existingKeys.Any(key =>
                key.ClientId == clientId
                && key.CityId == cityId
                && key.Street == buildingData.Street
                && key.Number == buildingData.Number);

            if (alreadyExists)
            {
                continue;
            }

            dbContext.Buildings.Add(new Building(
                clientId,
                cityId,
                buildingData.Street,
                buildingData.Number,
                buildingData.ConstructionYear,
                ParseBuildingType(buildingData.BuildingType),
                buildingData.Floors,
                buildingData.SurfaceArea,
                buildingData.InsuredValue,
                !buildingData.FloodRiskZone.Equals(
                    "Low",
                    StringComparison.OrdinalIgnoreCase),
                !buildingData.EarthquakeRiskZone.Equals(
                    "Low",
                    StringComparison.OrdinalIgnoreCase)));

            existingKeys.Add(new
            {
                ClientId = clientId,
                CityId = cityId,
                Street = buildingData.Street,
                Number = buildingData.Number
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<BuildingSeedData>> LoadBuildingsAsync(
        string contentRootPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, _options.BasePath, _options.BuildingFile));

        return await JsonSerializer.DeserializeAsync<
            List<BuildingSeedData>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.BuildingFile} file is empty or invalid.");
    }

    private async Task<Dictionary<int, Guid>> LoadCitiesAsync(
        string contentRootPath,
        InsuranceDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, _options.BasePath, _options.GeographyFile));

        var geography = await JsonSerializer.DeserializeAsync<
            Dictionary<string, Dictionary<string, string>>>(
                stream,
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.GeographyFile} file is empty or invalid.");

        var cityEntities = await dbContext.Cities
            .Join(
                dbContext.Counties,
                city => city.CountyId,
                county => county.Id,
                (city, county) => new
                {
                    city.Id,
                    CountyName = county.Name,
                    CityName = city.Name
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cityIds = new Dictionary<int, Guid>();
        var sourceId = 1;

        foreach (var county in geography)
        {
            foreach (var city in county.Value)
            {
                var cityEntity = cityEntities.FirstOrDefault(entity =>
                    SeedText.Normalize(entity.CountyName)
                        == SeedText.Normalize(county.Key)
                    && SeedText.Normalize(entity.CityName)
                        == SeedText.Normalize(city.Key));

                if (cityEntity is null)
                {
                    throw new InvalidOperationException(
                        $"City '{city.Key}' in county '{county.Key}' was not found.");
                }

                cityIds[sourceId++] = cityEntity.Id;
            }
        }

        return cityIds;
    }

    private static BuildingType ParseBuildingType(string value)
    {
        if (Enum.TryParse<BuildingType>(value, ignoreCase: true, out var type))
        {
            return type;
        }

        throw new InvalidOperationException(
            $"Unsupported building type '{value}'.");
    }

    private sealed class BuildingSeedData
    {
        public int BuildingId { get; set; }
        public int ClientId { get; set; }
        public int CityId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int ConstructionYear { get; set; }
        public string BuildingType { get; set; } = string.Empty;
        public int Floors { get; set; }
        public decimal SurfaceArea { get; set; }
        public decimal InsuredValue { get; set; }
        public string FloodRiskZone { get; set; } = string.Empty;
        public string EarthquakeRiskZone { get; set; } = string.Empty;
    }
}
