namespace InsuranceApp.Api.Configurations;

internal sealed class CorsSettings
{
    public const string SectionName = "CorsSettings";

    public string PolicyName { get; set; } = string.Empty;

    public string[] AllowedOrigins { get; set; } = [];
}