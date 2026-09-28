using System.Text.Json;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Seed;

public sealed class CurrencySeeder
{
    private readonly SeedDataOptions _options;

    public CurrencySeeder(IOptions<SeedDataOptions> options)
    {
        _options = options.Value;
    }

    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, _options.BasePath, _options.CurrencyFile));

        var currencies = await JsonSerializer.DeserializeAsync<
            List<CurrencySeedData>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.CurrencyFile} file is empty or invalid.");

        var existingCodes = await dbContext.Currencies
            .Select(currency => currency.Code)
            .ToHashSetAsync(cancellationToken);

        foreach (var currencyData in currencies)
        {
            if (!existingCodes.Add(currencyData.Code))
            {
                continue;
            }

            dbContext.Currencies.Add(new Currency(
                currencyData.Code,
                currencyData.Name,
                currencyData.ExchangeRateToBase,
                currencyData.IsActive));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class CurrencySeedData
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal ExchangeRateToBase { get; set; }
        public bool IsActive { get; set; }
    }
}
