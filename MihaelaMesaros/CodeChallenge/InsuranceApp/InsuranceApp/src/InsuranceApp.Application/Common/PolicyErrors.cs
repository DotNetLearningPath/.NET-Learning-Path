using InsuranceApp.Domain.Constants;

namespace InsuranceApp.Application.Common;

public static class PolicyErrors
{
    public static readonly Error InvalidPolicyId = new("Policy.InvalidPolicyId", "Policy Id is required.", ErrorType.Validation);

    public static readonly Error InvalidPageNumber = new("Policy.InvalidPageNumber", "Page number is invalid.", ErrorType.Validation);

    public static readonly Error InvalidPageSize = new("Policy.InvalidPageSize", "Page size is invalid.", ErrorType.Validation);

    public static readonly Error InvalidStatus = new("Policy.InvalidStatus", "Policy status is invalid.", ErrorType.Validation);

    public static readonly Error InvalidDateRange = new("Policy.InvalidDateRange", "Start date cannot be after end date.", ErrorType.Validation);

    public static Error NotFound(Guid policyId) => new("Policy.NotFound", $"Policy '{policyId}' was not found.", ErrorType.NotFound);

    public static readonly Error InvalidClientId = new("Policy.InvalidClientId", "Client Id is required.", ErrorType.Validation);

    public static readonly Error InvalidBuildingId = new("Policy.InvalidBuildingId", "Building Id is required.", ErrorType.Validation);

    public static readonly Error InvalidBrokerId = new("Policy.InvalidBrokerId", "Broker Id is required.", ErrorType.Validation);

    public static readonly Error InvalidCurrencyId = new("Policy.InvalidCurrencyId", "Currency Id is required.", ErrorType.Validation);

    public static readonly Error InvalidBasePremium = new("Policy.InvalidBasePremium", "Base premium must be greater than zero.", ErrorType.Validation);

    public static readonly Error InvalidBasePremiumScale = new("Policy.InvalidBasePremiumScale", $"Base premium must have a maximum of {PolicyConstraints.PremiumScale} decimal places.", ErrorType.Validation);

    public static readonly Error ClientNotFound = new("Policy.ClientNotFound", "Client was not found.", ErrorType.NotFound);

    public static readonly Error BuildingNotFound = new("Policy.BuildingNotFound", "Building was not found.", ErrorType.NotFound);

    public static readonly Error BuildingDoesNotBelongToClient = new("Policy.BuildingDoesNotBelongToClient", "The building does not belong to the selected client.", ErrorType.Validation);

    public static readonly Error BrokerNotFound = new("Policy.BrokerNotFound", "Broker was not found.", ErrorType.NotFound);

    public static readonly Error InactiveBroker = new("Policy.InactiveBroker", "The broker is not active and cannot create/activate policies.", ErrorType.Validation);

    public static readonly Error CurrencyNotFound = new("Policy.CurrencyNotFound", "Currency was not found.", ErrorType.NotFound);

    public static readonly Error InactiveCurrency = new("Policy.InactiveCurrency", "The selected currency is inactive.", ErrorType.Validation);

    public static readonly Error PolicyReferencesValidationFailed = new("Policy.PolicyReferencesValidationFailed", "Policy references validation failed.", ErrorType.Validation);
    public static readonly Error PolicyNumberNotGenerated = new("Policy.PolicyNumberNotGenerated", "Could not generate a valid policy number.", ErrorType.Conflict);
    public static readonly Error PolicyNotDraft = new("Policy.PolicyNotDraft", "This policy cannot be activated because it is not in draft status.", ErrorType.Validation);
    public static readonly Error StartDateInPast = new("Policy.StartDateInPast", "Policy start date must be in the future.", ErrorType.Validation);
    public static readonly Error OverlappingPolicyExists = new("Policy.OverlappingPolicyExists", "There already exists an active policy for the selected building and period.", ErrorType.Conflict);
    

    public static readonly Error CancellationInactivePolicy = new("Policy.CancellationInactivePolicy", "Policy is not active and cannot be cancelled.", ErrorType.Validation);
    public static readonly Error CancellationExpiredPolicy = new("Policy.CancellationExpiredPolicy", "Policy is expired and cannot be cancelled.", ErrorType.Validation);
    public static readonly Error CancellationReasonRequired = new("Policy.CancellationReasonRequired", "Cancellation reason is required.", ErrorType.Validation);
    public static readonly Error InvalidCancellationReasonMaxLength = new("Policy.InvalidCancellationReasonMaxLength", $"Cancellation reason must have maximum {PolicyConstraints.CancellationReasonMaxLength} characters.", ErrorType.Validation);
}
