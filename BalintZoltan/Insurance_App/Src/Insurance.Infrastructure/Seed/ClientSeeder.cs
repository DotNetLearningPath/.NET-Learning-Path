using System.Text.Json;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Insurance.Infrastructure.Seed;

public sealed class ClientSeeder(IOptions<SeedDataOptions> options)
{
    public async Task SeedAsync(
        InsuranceDbContext dbContext,
        string contentRootPath,
        CancellationToken cancellationToken)
    {
        var clients = await LoadAsync(contentRootPath, cancellationToken);
        var existingIdentifiers = await dbContext.Clients
            .Select(client => client.IdentificationNumber)
            .ToHashSetAsync(cancellationToken);

        foreach (var clientData in clients)
        {
            if (!existingIdentifiers.Add(clientData.Identifier))
            {
                continue;
            }

            dbContext.Clients.Add(new Client(
                ParseClientType(clientData.ClientType),
                clientData.Name,
                clientData.Identifier,
                clientData.Email,
                clientData.Phone,
                clientData.AddressLine));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClientSeedData>> LoadAsync(
        string contentRootPath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(
            SeedFilePath.Get(contentRootPath, options.Value.BasePath, options.Value.ClientFile));

        JsonSerializerOptions jsonSerializerClient = new()
        {
            PropertyNameCaseInsensitive = true
        };
        JsonSerializerOptions client = jsonSerializerClient;
        return await JsonSerializer.DeserializeAsync<
            List<ClientSeedData>>(
                stream,
                client,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"The {options.Value.ClientFile} file is empty or invalid.");
    }

    private static ClientType ParseClientType(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "PERSON" => ClientType.Individual,
            "COMPANY" => ClientType.Company,
            _ => throw new InvalidOperationException(
                $"Unsupported client type '{value}'.")
        };
    }

    public sealed class ClientSeedData
    {
        public string ClientType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
    }
}
