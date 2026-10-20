namespace InsuranceApp.Application.Policies.Commands;

public record CancelPolicyCommand(
    Guid PolicyId,
    string Reason,
    DateOnly EffectiveDate
);
