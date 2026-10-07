namespace InsuranceApp.Application.Common;

public static class PolicyErrors
{
    public static readonly Error InvalidPolicyId = new("Policy.InvalidPolicyId", "Policy id is required.", ErrorType.Validation);

    public static readonly Error InvalidPageNumber = new("Policy.InvalidPageNumber", "Page number is invalid.", ErrorType.Validation);

    public static readonly Error InvalidPageSize = new("Policy.InvalidPageSize", "Page size is invalid.", ErrorType.Validation);

    public static readonly Error InvalidStatus = new("Policy.InvalidStatus", "Policy status is invalid.", ErrorType.Validation);

    public static readonly Error InvalidDateRange = new("Policy.InvalidDateRange", "Start date cannot be after end date.", ErrorType.Validation);

    public static Error NotFound(Guid policyId) => new("Policy.NotFound", $"Policy '{policyId}' was not found.", ErrorType.NotFound);


}
