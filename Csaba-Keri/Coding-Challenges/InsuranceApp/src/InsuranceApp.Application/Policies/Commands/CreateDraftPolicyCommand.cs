namespace InsuranceApp.Application.Policies.Commands;

public record CreateDraftPolicyCommand(
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePremium
);
