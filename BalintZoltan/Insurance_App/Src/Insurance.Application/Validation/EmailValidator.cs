using System.ComponentModel.DataAnnotations;

namespace Insurance.Application.Validation;

internal static class EmailValidator
{
    private const int MaxEmailLength = 254;

    public static void ValidateFormatAndLength(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalizedEmail = email.Trim();

        if (normalizedEmail.Length > MaxEmailLength)
        {
            throw new ArgumentException(
                "Email cannot be longer than 254 characters.");
        }

        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new ArgumentException(
                "Invalid email address format.");
        }
    }
}
