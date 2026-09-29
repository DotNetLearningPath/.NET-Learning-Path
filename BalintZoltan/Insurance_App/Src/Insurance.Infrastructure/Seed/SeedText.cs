using System.Text;

namespace Infrastructure.Seed;

internal static class SeedText
{
    public static string Normalize(string value)
    {
        if (!value.Any(character => character is 'Ã' or 'Â' or 'Ä' or 'Å' or 'È'))
        {
            return value;
        }

        var repaired = Encoding.UTF8.GetString(
            Encoding.Latin1.GetBytes(value));

        return repaired.Contains('\uFFFD', StringComparison.Ordinal)
            ? value
            : repaired;
    }
}
