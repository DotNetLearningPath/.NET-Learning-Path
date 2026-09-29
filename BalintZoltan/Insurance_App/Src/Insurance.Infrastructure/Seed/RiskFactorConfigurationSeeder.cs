using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Seed;

public sealed class RiskFactorConfigurationSeeder
{
    private readonly SeedDataOptions _options;

    public RiskFactorConfigurationSeeder(IOptions<SeedDataOptions> options)
    {
        _options = options.Value;
    }

    public async Task SeedAsync(InsuranceDbContext dbContext, string contentRootPath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(SeedFilePath.Get(
            contentRootPath,
            _options.BasePath,
            _options.RiskFactorConfigurationFile));
        var configurations = await JsonSerializer.DeserializeAsync<List<RiskFactorSeedData>>(
            stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.RiskFactorConfigurationFile} file is empty or invalid.");

        var references = await LoadGeographyReferencesAsync(dbContext, cancellationToken);
        var existingConfigurations = await dbContext.RiskFactorConfigurations
            .Select(configuration => new { configuration.Level, configuration.Reference })
            .ToListAsync(cancellationToken);

        foreach (var configurationData in configurations)
        {
            if (!Enum.TryParse<RiskFactorLevel>(configurationData.Level, true, out var level))
                throw new InvalidOperationException($"Unsupported risk factor level '{configurationData.Level}'.");

            var reference = ResolveReference(level, configurationData.Reference, references);
            if (existingConfigurations.Any(configuration => configuration.Level == level && configuration.Reference == reference))
                continue;

            dbContext.RiskFactorConfigurations.Add(new RiskFactorConfiguration(
                level, reference, configurationData.AdjustmentPercentage, configurationData.IsActive));
            existingConfigurations.Add(new { Level = level, Reference = reference });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<(RiskFactorLevel Level, string Name), string>> LoadGeographyReferencesAsync(
        InsuranceDbContext dbContext, CancellationToken cancellationToken)
    {
        var countries = await dbContext.Countries.AsNoTracking().ToListAsync(cancellationToken);
        var counties = await dbContext.Counties.AsNoTracking().ToListAsync(cancellationToken);
        var cities = await dbContext.Cities.AsNoTracking().ToListAsync(cancellationToken);
        var references = new Dictionary<(RiskFactorLevel Level, string Name), string>();

        foreach (var country in countries)
            references.TryAdd((RiskFactorLevel.Country, NormalizeGeographyReference(country.Name)), country.Id.ToString());
        foreach (var county in counties)
            references.TryAdd((RiskFactorLevel.County, NormalizeGeographyReference(county.Name)), county.Id.ToString());
        foreach (var city in cities)
            references.TryAdd((RiskFactorLevel.City, NormalizeGeographyReference(city.Name)), city.Id.ToString());

        return references;
    }

    private static string ResolveReference(
        RiskFactorLevel level, string reference,
        IReadOnlyDictionary<(RiskFactorLevel Level, string Name), string> geographyReferences)
    {
        if (level == RiskFactorLevel.BuildingType)
        {
            if (!Enum.TryParse<BuildingType>(SeedText.Normalize(reference), true, out var buildingType))
                throw new InvalidOperationException($"Unsupported building type reference '{reference}'.");
            return buildingType.ToString();
        }

        var normalizedReference = NormalizeGeographyReference(reference);
        if (geographyReferences.TryGetValue((level, normalizedReference), out var id))
            return id;

        throw new InvalidOperationException($"Geography reference '{reference}' was not found for level '{level}'.");
    }

    private static string NormalizeGeographyReference(string value)
    {
        var normalized = SeedText.Normalize(value).Trim().ToUpperInvariant().Replace('Ţ', 'Ț').Replace('Ş', 'Ș');
        return normalized.StartsWith("JUDEȚUL ", StringComparison.Ordinal)
            ? normalized["JUDEȚUL ".Length..]
            : normalized;
    }

    private sealed class RiskFactorSeedData
    {
        public string Level { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
