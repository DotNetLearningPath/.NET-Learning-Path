using System.Net.Mail;

namespace InsuranceApp.Domain.Clients;

public static class EmailAddressRules
{
    public static bool IsSingleAddress(string? email)
    {
        return MailAddress.TryCreate(email, out var parsedAddress)
            && string.Equals(parsedAddress.Address, email, StringComparison.Ordinal);
    }
}
