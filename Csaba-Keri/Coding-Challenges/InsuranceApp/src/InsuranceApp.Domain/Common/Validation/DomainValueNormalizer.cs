namespace InsuranceApp.Domain.Common.Validation;

public static class DomainValueNormalizer
{
    public static string NormalizeRequired(
        string value,
        int maxLength,
        string parameterName
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be null or whitespace.",
                parameterName
            );
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value must not exceed {maxLength} characters.",
                parameterName
            );
        }

        return normalizedValue;
    }

    public static string NormalizeEmail(
        string email,
        int maxLength,
        string parameterName
    )
    {
        var normalizedEmail = NormalizeRequired(email, maxLength, parameterName);

        if (!EmailAddressRules.IsSingleAddress(normalizedEmail))
        {
            throw new ArgumentException(
                "A single email address without a display name is required.",
                parameterName
            );
        }

        return normalizedEmail;
    }
}
