using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Results;

public record PolicyListItemResult(
    Guid Id,
    string PolicyNumber,
    PolicyStatus Status,
    Guid ClientId,
    Guid BuildingId,
    Guid BrokerId,
    Guid CurrencyId,
    string CurrencyCode,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal BasePremium,
    decimal FinalPremium,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
