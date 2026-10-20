using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Insurance.Infrastructure.Seed;

public sealed class BrokerSeeder(IOptions<SeedDataOptions> options)
{
    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, options.Value.BasePath, options.Value.BrokerFile));

        JsonSerializerOptions jsonSerializerBroker = new()
        {
            PropertyNameCaseInsensitive = true
        };
        JsonSerializerOptions broker = jsonSerializerBroker;
        var brokers = await JsonSerializer.DeserializeAsync<
            List<BrokerSeedData>>(
                stream,
                broker,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {options.Value.BrokerFile} file is empty or invalid.");

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
