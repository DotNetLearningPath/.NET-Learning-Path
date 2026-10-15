using InsuranceApp.Application.Abstractions.Services;
using System.Text;

namespace InsuranceApp.Application.Services;

public sealed class PolicyNumberGenerator : IPolicyNumberGenerator
{
    public string GeneratePolicyNumber()
    {
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int noChars = 10;

        var randomPart = new StringBuilder(noChars);
        for (var i = 0; i < noChars; i++)
        {
            var randomIndex = Random.Shared.Next(characters.Length);
            randomPart.Append(characters[randomIndex]);
        }

        var finalGenerated = $"{DateTime.UtcNow.Year}-{randomPart}";

        return finalGenerated.ToUpperInvariant();
    }
}
