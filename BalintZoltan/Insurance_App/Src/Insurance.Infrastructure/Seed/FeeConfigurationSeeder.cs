using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Seed;

public sealed class FeeConfigurationSeeder
{
    private readonly SeedDataOptions _options;

    public FeeConfigurationSeeder(IOptions<SeedDataOptions> options)
    {
        _options = options.Value;
    }

    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, _options.BasePath, _options.FeeConfigurationFile));

        var configurations = await JsonSerializer.DeserializeAsync<
            List<FeeConfigurationSeedData>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.FeeConfigurationFile} file is empty or invalid.");

        var existingConfigurations = await dbContext.FeeConfigurations
            .Select(configuration => new
            {
                configuration.Name,
                configuration.Type,
                configuration.EffectiveFrom
            })
            .ToListAsync(cancellationToken);

        foreach (var configurationData in configurations)
        {
            if (!Enum.TryParse<FeeType>(
                    configurationData.Type,
                    ignoreCase: true,
                    out var type))
            {
                throw new InvalidOperationException(
                    $"Unsupported fee type '{configurationData.Type}'.");
            }

            var exists = existingConfigurations.Any(configuration =>
                configuration.Name == configurationData.Name
                && configuration.Type == type
                && configuration.EffectiveFrom == configurationData.EffectiveFrom);

            if (exists)
            {
                continue;
            }

            dbContext.FeeConfigurations.Add(new FeeConfiguration(
                configurationData.Name,
                type,
                configurationData.Percentage,
                configurationData.EffectiveFrom,
                configurationData.EffectiveTo,
                configurationData.IsActive));

            existingConfigurations.Add(new
            {
                configurationData.Name,
                Type = type,
                configurationData.EffectiveFrom
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class FeeConfigurationSeedData
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }
}
