using InsuranceApp.Application.Abstractions.Services;

namespace InsuranceApp.Application.Services;

public sealed class PolicyNumberGenerator : IPolicyNumberGenerator
{
    public string GeneratePolicyNumber()
    {
        //return "2026-TEST123456";

        var fromString = Guid.NewGuid().ToString("N")[..10];
        var finalGenerated = $"{DateTime.UtcNow:yyyy}-{fromString}";

        return finalGenerated.ToUpperInvariant();
    }
}
