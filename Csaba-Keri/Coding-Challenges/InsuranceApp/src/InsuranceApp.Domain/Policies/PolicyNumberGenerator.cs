namespace InsuranceApp.Domain.Policies;

public static class PolicyNumberGenerator
{
    public static string Generate(Guid policyId)
    {
        if (policyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Policy identifier must not be empty.",
                nameof(policyId)
            );
        }

        return $"POL-{policyId:N}".ToUpperInvariant();
    }
}
