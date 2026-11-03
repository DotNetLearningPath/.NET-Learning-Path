namespace InsuranceApp.Infrastructure.Persistence;

public sealed class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";

    public int CommandTimeoutSeconds { get; set; } = 30;
}
