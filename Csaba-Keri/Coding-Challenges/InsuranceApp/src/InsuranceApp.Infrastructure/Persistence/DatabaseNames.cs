namespace InsuranceApp.Infrastructure.Persistence;

internal static class DatabaseNames
{
    public const string ClientIdentifierIndex = "ux_clients_identification_number";
    public const string BrokerCodeIndex = "ux_brokers_code";
    public const string CurrencyCodeIndex = "ux_currencies_code";
    public const string BuildingClientForeignKey = "fk_buildings_clients_client_id";
    public const string BuildingCityForeignKey = "fk_buildings_cities_city_id";
    public const string RiskFactorCountryForeignKey = "fk_risk_factor_configurations_country_id";
    public const string RiskFactorCountyForeignKey = "fk_risk_factor_configurations_county_id";
    public const string RiskFactorCityForeignKey = "fk_risk_factor_configurations_city_id";
    public const string PolicyClientForeignKey = "fk_policies_client_id";
    public const string PolicyBuildingForeignKey = "fk_policies_building_id";
    public const string PolicyBrokerForeignKey = "fk_policies_broker_id";
    public const string PolicyCurrencyForeignKey = "fk_policies_currency_id";
}
