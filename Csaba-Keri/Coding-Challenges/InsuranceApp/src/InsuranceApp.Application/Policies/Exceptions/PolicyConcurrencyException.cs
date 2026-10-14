namespace InsuranceApp.Application.Policies.Exceptions;

public class PolicyConcurrencyException(Guid policyId)
    : Exception($"Policy '{policyId}' was modified or removed by another operation. Reload it before retrying.")
{
}
