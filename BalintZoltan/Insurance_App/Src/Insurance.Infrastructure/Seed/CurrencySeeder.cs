using System.Text.Json;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Insurance.Infrastructure.Seed;

public sealed class CurrencySeeder(IOptions<SeedDataOptions> options)
{
    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, options.Value.BasePath, options.Value.CurrencyFile));

        JsonSerializerOptions jsonSerializerCurrency = new()
        {
            PropertyNameCaseInsensitive = true
        };
        JsonSerializerOptions currency = jsonSerializerCurrency;
        var currencies = await JsonSerializer.DeserializeAsync<
            List<CurrencySeedData>>(
                stream,
                currency,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {options.Value.CurrencyFile} file is empty or invalid.");

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
