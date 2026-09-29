using System.Text.Json;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Seed;

public sealed class BrokerSeeder
{
    private readonly SeedDataOptions _options;

    public BrokerSeeder(IOptions<SeedDataOptions> options)
    {
        _options = options.Value;
    }

    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, _options.BasePath, _options.BrokerFile));

        var brokers = await JsonSerializer.DeserializeAsync<
            List<BrokerSeedData>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {_options.BrokerFile} file is empty or invalid.");

        var existingCodes = await dbContext.Brokers
            .Select(broker => broker.BrokerCode)
            .ToHashSetAsync(cancellationToken);

        foreach (var brokerData in brokers)
        {
            if (!existingCodes.Add(brokerData.BrokerCode))
            {
                continue;
            }

            if (!Enum.TryParse<BrokerStatus>(
                    brokerData.Status,
                    ignoreCase: true,
                    out var status))
            {
                throw new InvalidOperationException(
                    $"Unsupported broker status '{brokerData.Status}'.");
            }

            dbContext.Brokers.Add(new Broker(
                brokerData.BrokerCode,
                brokerData.Name,
                brokerData.Email,
                brokerData.Phone,
                status,
                brokerData.CommissionPercentage));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class BrokerSeedData
    {
        public string BrokerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? CommissionPercentage { get; set; }
    }
}
