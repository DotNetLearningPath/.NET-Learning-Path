namespace InsuranceApp.Infrastructure.Persistence;

internal static class DatabaseNames
{
    public const string ClientIdentifierIndex = "ux_clients_identification_number";
    public const string BuildingClientForeignKey = "fk_buildings_clients_client_id";
    public const string BuildingCityForeignKey = "fk_buildings_cities_city_id";
}
