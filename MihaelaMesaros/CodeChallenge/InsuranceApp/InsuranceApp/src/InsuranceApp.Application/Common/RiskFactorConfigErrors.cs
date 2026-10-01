using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class RiskFactorConfigErrors
{
    public static readonly Error InvalidRiskFactorConfigId = new(
        "RiskFactorConfig.InvalidRiskFactorConfigId",
        "Risk factor configuration ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error InvalidLevel = new(
        "RiskFactorConfig.InvalidLevel",
        "Risk factor level is invalid.",
        ErrorType.Validation);

    public static readonly Error InvalidReferenceId = new(
        "RiskFactorConfig.InvalidReferenceId",
        "Reference ID must not be empty.",
        ErrorType.Validation);

    public static readonly Error ReferenceNotFound = new(
        "RiskFactorConfig.ReferenceNotFound",
        "The referenced entity was not found.",
        ErrorType.NotFound);

    public static readonly Error InvalidAdjustmentPercentage = new(
        "RiskFactorConfig.InvalidAdjustmentPercentage",
        $"Adjustment percentage must be between {RiskFactorConfigConstraints.MinAdjustmentPercentage} and {RiskFactorConfigConstraints.MaxAdjustmentPercentage}.",
        ErrorType.Validation);

    public static readonly Error InvalidAdjustmentPercentageScale = new(
        "RiskFactorConfig.InvalidAdjustmentPercentageScale",
        $"Adjustment percentage must have a maximum of {RiskFactorConfigConstraints.AdjustmentPercentageScale} decimal places.",
        ErrorType.Validation);

    public static readonly Error AlreadyExists = new(
        "RiskFactorConfig.AlreadyExists",
        "A risk factor configuration already exists for the specified level and reference.",
        ErrorType.Conflict);

    public static Error NotFound(Guid riskFactorConfigId) => new(
        "RiskFactorConfig.NotFound",
        $"Risk factor configuration with ID {riskFactorConfigId} was not found.",
        ErrorType.NotFound);

}