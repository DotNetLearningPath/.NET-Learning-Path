namespace Infrastructure.Seed;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public string BasePath { get; set; } = string.Empty;
    public string GeographyFile { get; set; } = string.Empty;
    public string ClientFile { get; set; } = string.Empty;
    public string BuildingFile { get; set; } = string.Empty;
    public string CurrencyFile { get; set; } = string.Empty;
    public string BrokerFile { get; set; } = string.Empty;
    public string FeeConfigurationFile { get; set; } = string.Empty;
    public string RiskFactorConfigurationFile { get; set; } = string.Empty;
}
