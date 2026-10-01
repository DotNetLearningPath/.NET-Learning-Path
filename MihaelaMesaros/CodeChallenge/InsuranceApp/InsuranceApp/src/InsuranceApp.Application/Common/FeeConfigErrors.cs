using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class FeeConfigErrors
{
    public static readonly Error InvalidFeeConfigId = new(
        "FeeConfig.InvalidFeeConfigId",
        "Fee configuration ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error NameRequired = new(
        "FeeConfig.NameRequired",
        "Fee configuration name is required.",
        ErrorType.Validation);

    public static readonly Error InvalidNameLength = new(
        "FeeConfig.InvalidNameLength",
        $"Fee configuration name must be between {FeeConfigConstraints.NameMinLength} and {FeeConfigConstraints.NameMaxLength} characters.",
        ErrorType.Validation);

    public static readonly Error InvalidType = new(
        "FeeConfig.InvalidType",
        "Fee configuration type is invalid.",
        ErrorType.Validation);

    public static readonly Error InvalidPercentage = new(
        "FeeConfig.InvalidPercentage",
        $"Percentage must be between {FeeConfigConstraints.MinPercentage} and {FeeConfigConstraints.MaxPercentage}.",
        ErrorType.Validation);

    public static readonly Error InvalidPercentageScale = new(
        "FeeConfig.InvalidPercentageScale",
        $"Percentage must have a maximum of {FeeConfigConstraints.PercentageScale} decimal places.",
        ErrorType.Validation);

    public static readonly Error InvalidEffectivePeriod = new(
        "FeeConfig.InvalidEffectivePeriod",
        "Effective To must be greater than or equal to Effective From.",
        ErrorType.Validation);

    public static Error NotFound(Guid feeConfigId) => new(
        "FeeConfig.NotFound",
        $"Fee configuration with ID {feeConfigId} was not found.",
        ErrorType.NotFound);
}