namespace InsuranceApp.Application.DTOs.Policy;

public sealed record CreatePolicyDto(
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    DateTime StartDate,
    DateTime EndDate,
    decimal BasePremium
);
