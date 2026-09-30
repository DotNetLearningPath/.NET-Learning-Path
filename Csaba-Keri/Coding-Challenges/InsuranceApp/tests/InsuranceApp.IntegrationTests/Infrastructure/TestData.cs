using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests.Infrastructure;

internal static class TestData
{
    public const string RomaniaId = "11111111-1111-4111-8111-111111111111";
    public const string ClujId = "22222222-2222-4222-8222-222222222222";
    public const string ClujNapocaId = "33333333-3333-4333-8333-333333333333";
    public const string DebrecenId = "33333333-3333-4333-8333-333333333335";

    public static JsonObject Client(
        string type = "Individual",
        string? identificationNumber = null,
        string name = "Test Client",
        string email = "test@example.com",
        string phone = "+40 700 123 456",
        string? primaryAddress = "Main Street 10"
    ) => new()
    {
        ["type"] = type,
        ["identificationNumber"] = identificationNumber ?? Guid.NewGuid().ToString(),
        ["name"] = name,
        ["email"] = email,
        ["phone"] = phone,
        ["primaryAddress"] = primaryAddress
    };

    public static JsonObject BuildingAddress(
        string cityId = ClujNapocaId,
        string street = "Main Street",
        string number = "12A"
    ) => new()
    {
        ["cityId"] = cityId,
        ["street"] = street,
        ["number"] = number
    };

    public static JsonObject Building(
        string type = "Residential",
        string cityId = ClujNapocaId,
        string street = "Main Street",
        string number = "12A",
        int constructionYear = 2005,
        int numberOfFloors = 2,
        decimal surfaceArea = 125.50m,
        decimal insuredValue = 250000m
    ) => new()
    {
        ["type"] = type,
        ["address"] = BuildingAddress(cityId, street, number),
        ["constructionYear"] = constructionYear,
        ["numberOfFloors"] = numberOfFloors,
        ["surfaceArea"] = surfaceArea,
        ["insuredValue"] = insuredValue
    };

    public static JsonObject Broker(
        string code = "BR-001",
        string name = "Test Broker",
        string email = "broker@example.com",
        string phone = "123",
        string status = "Active"
    ) => new()
    {
        ["code"] = code,
        ["name"] = name,
        ["email"] = email,
        ["phone"] = phone,
        ["status"] = status
    };
}
