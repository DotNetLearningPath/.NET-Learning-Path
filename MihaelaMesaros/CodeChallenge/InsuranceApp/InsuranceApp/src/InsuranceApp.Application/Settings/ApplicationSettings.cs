namespace InsuranceApp.Application.Settings;

public sealed class ApplicationSettings
{
    public const string SectionName = "ApplicationSettings";

    public string BaseCurrencyCode { get; set; } = string.Empty;
}

