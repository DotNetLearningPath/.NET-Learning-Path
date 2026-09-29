using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class CurrencyErrors
{
    public static readonly Error InvalidCurrencyId = new(
        "Currency.InvalidCurrencyId",
        "Currency ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error CodeRequired = new(
        "Currency.CodeRequired",
        "Currency code is required.",
        ErrorType.Validation);

    public static readonly Error InvalidCodeLength = new(
        "Currency.InvalidCodeLength",
        $"Currency code must contain exactly {CurrencyConstraints.CodeMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error NameRequired = new(
        "Currency.NameRequired",
        "Currency name is required.",
        ErrorType.Validation);

    public static readonly Error InvalidNameLength = new(
        "Currency.InvalidNameLength",
        $"Currency name must be between {CurrencyConstraints.NameMinLength} and {CurrencyConstraints.NameMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidExchangeRate = new(
        "Currency.InvalidExchangeRate",
        $"Exchange rate must be between {CurrencyConstraints.MinExchangeRate} and {CurrencyConstraints.MaxExchangeRate}.",
        ErrorType.Validation);

    public static readonly Error InvalidExchangeRateScale = new(
        "Currency.InvalidExchangeRateScale",
        $"Exchange rate must have a maximum of {CurrencyConstraints.ExchangeRateScale} decimal places.",
        ErrorType.Validation);

    public static readonly Error DuplicateCode = new(
        "Currency.DuplicateCode",
        "A currency with this code already exists.",
        ErrorType.Conflict);

    public static Error NotFound(Guid currencyId) => new(
        "Currency.NotFound",
        $"Currency with ID {currencyId} was not found.",
        ErrorType.NotFound);
}